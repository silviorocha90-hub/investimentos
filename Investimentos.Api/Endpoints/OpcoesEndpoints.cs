using Investimentos.Application.Interfaces;
using Investimentos.Application.Opcoes.AtualizarOperacaoOpcao;
using Investimentos.Application.Opcoes.CadastrarOperacaoOpcao;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Opcoes.ExcluirOperacaoOpcao;
using Investimentos.Domain.Entities;

namespace Investimentos.Api.Endpoints
{
    public static class OpcoesEndpoints
    {
        public static WebApplication MapOpcoesEndpoints(
            this WebApplication app)
        {
            app.MapPost(
                "/api/opcoes",
                async (
                    CadastrarOpcaoRequest request,
                    CadastrarOperacaoOpcaoHandler cadastrarHandler,
                    ConsultarOpcoesHandler consultarHandler,
                    IOperacaoOpcaoCrudRepository crudRepository,
                    CancellationToken cancellationToken) =>
                {
                    if (request.InvestidorId == Guid.Empty)
                    {
                        return Results.BadRequest(
                            new
                            {
                                detail =
                                    "O investidor é obrigatório."
                            });
                    }

                    var tickerOpcao =
                        request.Ticker
                            .Trim()
                            .ToUpperInvariant();

                    var historico =
                        await consultarHandler.HandleAsync(
                            request.InvestidorId,
                            cancellationToken);

                    var raizNova =
                        ObterRaizOpcao(
                            tickerOpcao);

                    var ativosCandidatos =
                        historico
                            .Where(x =>
                                ObterRaizOpcao(
                                    x.TickerOpcao) ==
                                raizNova)
                            .Select(x =>
                                x.TickerAtivo)
                            .Distinct(
                                StringComparer
                                    .OrdinalIgnoreCase)
                            .ToList();

                    if (ativosCandidatos.Count == 0)
                    {
                        return Results.BadRequest(
                            new
                            {
                                detail =
                                    $"Não foi possível identificar automaticamente o ativo-base da opção {tickerOpcao}. Cadastre primeiro uma associação histórica para esse ativo."
                            });
                    }

                    if (ativosCandidatos.Count > 1)
                    {
                        return Results.BadRequest(
                            new
                            {
                                detail =
                                    $"Existem múltiplos ativos-base associados à raiz {raizNova}. Não é possível escolher automaticamente com segurança."
                            });
                    }

                    var tickerAtivo =
                        ativosCandidatos[0];

                    var command =
                        new CadastrarOperacaoOpcaoCommand(
                            request.InvestidorId,
                            tickerAtivo,
                            tickerOpcao,
                            request.TipoOpcao,
                            request.Natureza,
                            request.DataOperacao,
                            request.Vencimento,
                            request.Strike,
                            1,
                            request.Quantidade,
                            request.PremioUnitario,
                            0);

                    var id =
                        await cadastrarHandler.HandleAsync(
                            command,
                            cancellationToken);

                    var operacao =
                        await crudRepository
                            .ObterPorIdAsync(
                                id,
                                cancellationToken);

                    if (operacao is null)
                    {
                        throw new InvalidOperationException(
                            "A opção foi criada, mas não pôde ser recuperada para finalização.");
                    }

                    operacao.MarcarExercida();

                    await crudRepository
                        .SalvarAlteracoesAsync(
                            cancellationToken);

                    return Results.Created(
                        $"/api/opcoes/{id}",
                        new
                        {
                            id
                        });
                });

            app.MapGet(
                "/api/opcoes/{investidorId:guid}",
                async (
                    Guid investidorId,
                    ConsultarOpcoesHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var resultado =
                        await handler.HandleAsync(
                            investidorId,
                            cancellationToken);

                    return Results.Ok(
                        resultado);
                });

            app.MapPut(
                "/api/opcoes/{id:guid}",
                async (
                    Guid id,
                    AtualizarOpcaoRequest request,
                    AtualizarOperacaoOpcaoHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var command =
                        new AtualizarOperacaoOpcaoCommand(
                            id,
                            request.DataOperacao,
                            request.Vencimento,
                            request.Strike,
                            request.Contratos,
                            request.Quantidade,
                            request.PremioUnitario,
                            0,
                            request.Situacao,
                            request.DataFinalizacao,
                            request.PrecoRecompraUnitario,
                            request.ValorExecucao);

                    await handler.HandleAsync(
                        command,
                        cancellationToken);

                    return Results.NoContent();
                });

            app.MapDelete(
                "/api/opcoes/{id:guid}",
                async (
                    Guid id,
                    ExcluirOperacaoOpcaoHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(
                        id,
                        cancellationToken);

                    return Results.NoContent();
                });

            return app;
        }

        private static string ObterRaizOpcao(
            string ticker)
        {
            var tickerNormalizado =
                ticker
                    .Trim()
                    .ToUpperInvariant();

            var primeiraPosicaoNumerica =
                tickerNormalizado
                    .TakeWhile(
                        char.IsLetter)
                    .Count();

            var parteLetras =
                tickerNormalizado[
                    ..primeiraPosicaoNumerica];

            if (parteLetras.Length <= 1)
            {
                return parteLetras;
            }

            /*
             * O último caractere alfabético antes da
             * parte numérica representa o código
             * de vencimento da opção.
             *
             * Exemplo:
             * ITUBU407 -> ITUB
             * VALEM631 -> VALE
             */
            return parteLetras[..^1];
        }
    }

    public record CadastrarOpcaoRequest(
        Guid InvestidorId,
        string Ticker,
        string TipoOpcao,
        string Natureza,
        DateTime DataOperacao,
        DateTime Vencimento,
        decimal Quantidade,
        decimal Strike,
        decimal PremioUnitario);

    public record AtualizarOpcaoRequest(
        DateTime DataOperacao,
        DateTime Vencimento,
        decimal Strike,
        int Contratos,
        decimal Quantidade,
        decimal PremioUnitario,
        string Situacao,
        DateTime? DataFinalizacao,
        decimal? PrecoRecompraUnitario,
        decimal? ValorExecucao);
}