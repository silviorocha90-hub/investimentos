using Investimentos.Application.Administracao;
using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence.Repositories;

namespace Investimentos.Api.Endpoints
{
    public static class AdministracaoInvestidoresEndpoints
    {
        public static IEndpointRouteBuilder
            MapAdministracaoInvestidoresEndpoints(
                this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPut(
                "/api/admin/investidores/{id:guid}",
                AtualizarInvestidorAsync);

            endpoints.MapPost(
                "/api/admin/investidores/{investidorId:guid}/historico-patrimonial",
                CriarHistoricoAsync);

            endpoints.MapPut(
                "/api/admin/historico-patrimonial/{id:guid}",
                AtualizarHistoricoAsync);

            endpoints.MapDelete(
                "/api/admin/historico-patrimonial/{id:guid}",
                ExcluirHistoricoAsync);

            return endpoints;
        }

        private static async Task<IResult>
            AtualizarInvestidorAsync(
                Guid id,
                AtualizarInvestidorAdministracaoRequest request,
                IInvestidorAdministracaoRepository investidorRepository,
                ISaldoDisponivelRepository saldoRepository,
                CancellationToken cancellationToken)
        {
            try
            {
                var investidor =
                    await investidorRepository.ObterPorIdAsync(
                        id,
                        cancellationToken);

                if (investidor is null)
                {
                    return Results.NotFound(
                        new
                        {
                            detail =
                                "Investidor não encontrado."
                        });
                }

                investidor.AlterarNome(
                    request.Nome);

                var saldo =
                    await saldoRepository.ObterAtualAsync(
                        id,
                        cancellationToken);

                if (saldo is null)
                {
                    saldo =
                        new SaldoDisponivel(
                            investidor,
                            DateTime.Today,
                            request.SaldoDisponivel);

                    await saldoRepository.AdicionarAsync(
                        saldo,
                        cancellationToken);
                }
                else
                {
                    saldo.Atualizar(
                        DateTime.Today,
                        request.SaldoDisponivel);
                }

                /*
                 * Os repositórios compartilham o mesmo
                 * DbContext Scoped nesta requisição.
                 * Um único SaveChanges persiste
                 * Investidor e SaldoDisponivel.
                 */
                await investidorRepository
                    .SalvarAlteracoesAsync(
                        cancellationToken);

                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(
                    new
                    {
                        detail = ex.Message
                    });
            }
        }

        private static async Task<IResult>
            CriarHistoricoAsync(
                Guid investidorId,
                CriarHistoricoPatrimonioAdministracaoRequest request,
                IInvestidorAdministracaoRepository investidorRepository,
                IHistoricoPatrimonioRepository historicoRepository,
                CancellationToken cancellationToken)
        {
            try
            {
                var investidor =
                    await investidorRepository.ObterPorIdAsync(
                        investidorId,
                        cancellationToken);

                if (investidor is null)
                {
                    return Results.NotFound(
                        new
                        {
                            detail =
                                "Investidor não encontrado."
                        });
                }

                var historico =
                    new HistoricoPatrimonio(
                        investidor,
                        request.DataReferencia,
                        request.ValorCarteira);

                await historicoRepository.AdicionarAsync(
                    historico,
                    cancellationToken);

                await historicoRepository
                    .SalvarAlteracoesAsync(
                        cancellationToken);

                return Results.Created(
                    $"/api/admin/historico-patrimonial/{historico.Id}",
                    new
                    {
                        historico.Id
                    });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(
                    new
                    {
                        detail = ex.Message
                    });
            }
        }

        private static async Task<IResult>
            AtualizarHistoricoAsync(
                Guid id,
                AtualizarHistoricoPatrimonioAdministracaoRequest request,
                IHistoricoPatrimonioRepository repository,
                CancellationToken cancellationToken)
        {
            try
            {
                var historico =
                    await repository.ObterPorIdAsync(
                        id,
                        cancellationToken);

                if (historico is null)
                {
                    return Results.NotFound(
                        new
                        {
                            detail =
                                "Registro de patrimônio não encontrado."
                        });
                }

                historico.Atualizar(
                    request.DataReferencia,
                    request.ValorCarteira);

                await repository.SalvarAlteracoesAsync(
                    cancellationToken);

                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(
                    new
                    {
                        detail = ex.Message
                    });
            }
        }

        private static async Task<IResult>
            ExcluirHistoricoAsync(
                Guid id,
                IHistoricoPatrimonioRepository repository,
                CancellationToken cancellationToken)
        {
            var historico =
                await repository.ObterPorIdAsync(
                    id,
                    cancellationToken);

            if (historico is null)
            {
                return Results.NotFound(
                    new
                    {
                        detail =
                            "Registro de patrimônio não encontrado."
                    });
            }

            repository.Excluir(
                historico);

            await repository.SalvarAlteracoesAsync(
                cancellationToken);

            return Results.NoContent();
        }
    }

    public record AtualizarInvestidorAdministracaoRequest(
        string Nome,
        decimal SaldoDisponivel);

    public record CriarHistoricoPatrimonioAdministracaoRequest(
        DateTime DataReferencia,
        decimal ValorCarteira);

    public record AtualizarHistoricoPatrimonioAdministracaoRequest(
        DateTime DataReferencia,
        decimal ValorCarteira);
}