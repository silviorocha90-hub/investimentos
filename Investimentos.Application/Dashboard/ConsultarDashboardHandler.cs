using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;

namespace Investimentos.Application.Dashboard
{
    public class ConsultarDashboardHandler
    {
        private readonly ConsultarCarteiraHandler _carteiraHandler;
        private readonly ConsultarProventosHandler _proventosHandler;
        private readonly ConsultarOpcoesHandler _opcoesHandler;
        private readonly ICotacaoAtivoRepository _cotacaoRepository;
        private readonly ISaldoDisponivelRepository _saldoRepository;
        private readonly IValorPatrimonialAtivoRepository _valorPatrimonialRepository;

        public ConsultarDashboardHandler(
            ConsultarCarteiraHandler carteiraHandler,
            ConsultarProventosHandler proventosHandler,
            ConsultarOpcoesHandler opcoesHandler,
            ICotacaoAtivoRepository cotacaoRepository,
            ISaldoDisponivelRepository saldoRepository,
            IValorPatrimonialAtivoRepository? valorPatrimonialRepository = null)
        {
            _carteiraHandler = carteiraHandler;
            _proventosHandler = proventosHandler;
            _opcoesHandler = opcoesHandler;
            _cotacaoRepository = cotacaoRepository;
            _saldoRepository = saldoRepository;
            _valorPatrimonialRepository =
                valorPatrimonialRepository ??
                new ValorPatrimonialAtivoRepositoryVazio();
        }

        public async Task<DashboardDto> HandleAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            var posicoesOriginais =
                await _carteiraHandler.HandleAsync(
                    investidorId,
                    cancellationToken);

            var resultadoRealizadoAcoes =
                await _carteiraHandler
                    .HandleResultadoRealizadoAsync(
                        investidorId,
                        cancellationToken);

            var proventos =
                await _proventosHandler.HandleAsync(
                    investidorId,
                    cancellationToken);

            var opcoes =
                await _opcoesHandler.HandleAsync(
                    investidorId,
                    cancellationToken);

            var cotacoes =
                await _cotacaoRepository
                    .ObterUltimasPorTickerAsync(
                        cancellationToken);

            var valoresPatrimoniais =
                await _valorPatrimonialRepository
                    .ObterUltimosPorTickerAsync(
                        investidorId,
                        cancellationToken);

            var posicoes =
                posicoesOriginais
                    .Select(x =>
                    {
                        if (EhAtivoSemMarcacaoPorCotacao(x))
                        {
                            var valorPatrimonialAtual =
                                valoresPatrimoniais.TryGetValue(
                                    x.Ticker,
                                    out var valorInformado)
                                    ? valorInformado
                                    : x.CustoTotal;

                            return x with
                            {
                                PrecoAtual = null,
                                ValorAtual = valorPatrimonialAtual,
                                Valorizacao =
                                    valorPatrimonialAtual -
                                    x.CustoTotal
                            };
                        }

                        if (!cotacoes.TryGetValue(
                            x.Ticker,
                            out var precoAtual))
                        {
                            return x with
                            {
                                PrecoAtual = null,
                                ValorAtual = 0,
                                Valorizacao = 0
                            };
                        }

                        var valorAtualPosicao =
                            x.Quantidade *
                            precoAtual;

                        return x with
                        {
                            PrecoAtual = precoAtual,
                            ValorAtual = valorAtualPosicao,
                            Valorizacao =
                                valorAtualPosicao -
                                x.CustoTotal
                        };
                    })
                    .ToList();

            var valorAplicado =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        x.TipoAtivoCodigo !=
                            "PREVIDENCIA")
                    .Sum(x =>
                        x.ValorAtual);

            var valorPrevidencia =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        x.TipoAtivoCodigo ==
                            "PREVIDENCIA")
                    .Sum(x =>
                        x.ValorAtual);

            var saldoAtual =
                await _saldoRepository
                    .ObterAtualAsync(
                        investidorId,
                        cancellationToken);

            var caixaDisponivel =
                saldoAtual?.Valor ?? 0;

            var patrimonioEstimado =
                valorAplicado +
                valorPrevidencia +
                caixaDisponivel;

            /*
             * PROVENTOS
             */
            var proventosBrutos =
                proventos.Sum(x =>
                    x.ValorBruto);

            var irProventos =
                proventos.Sum(x =>
                    x.IrEfetivo);

            var totalProventos =
                proventos.Sum(x =>
                    x.ValorLiquido);

            /*
             * OPÇÕES FINALIZADAS
             */
            var opcoesFinalizadas =
                opcoes
                    .Where(x =>
                        x.Situacao == "ENCERRADA" ||
                        x.Situacao == "EXECUTADA" ||
                        x.Situacao == "EXPIRADA")
                    .ToList();

            /*
             * Resultado operacional.
             *
             * IR estimado permanece apenas
             * como informação fiscal.
             */
            var opcoesBrutas =
                opcoesFinalizadas.Sum(x =>
                    x.ResultadoBruto ?? 0);

            var irEstimadoOpcoes =
                opcoesFinalizadas.Sum(x =>
                    x.IrEstimado);

            /*
             * Mantido com este nome para
             * compatibilidade com o frontend.
             *
             * No dashboard individual não existe
             * desconto fiscal global.
             */
            var premioLiquidoOpcoes =
                opcoesBrutas;

            /*
             * VALORIZAÇÃO NÃO REALIZADA
             */
            var valorizacaoAtivos =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        !EhAtivoSemMarcacaoPorCotacao(x) &&
                        x.PrecoAtual.HasValue)
                    .Sum(x =>
                        x.Valorizacao);

            /*
             * RESULTADO DA CARTEIRA
             *
             * Valorização não realizada
             * + vendas realizadas
             * + proventos líquidos
             * + opções realizadas.
             */
            var resultadoCarteira =
                valorizacaoAtivos +
                resultadoRealizadoAcoes +
                totalProventos +
                premioLiquidoOpcoes;

            const decimal descontosFiscais = 0;

            var quantidadeAtivos =
                posicoes.Count(x =>
                    x.Quantidade > 0 &&
                    x.TipoAtivoCodigo !=
                        "PREVIDENCIA");

            var quantidadeOpcoesAbertas =
                opcoes.Count(x =>
                    x.Situacao == "ABERTA");

            return new DashboardDto(
                valorAplicado,
                resultadoCarteira,
                totalProventos,
                premioLiquidoOpcoes,
                quantidadeAtivos,
                quantidadeOpcoesAbertas,
                posicoes,
                proventos,
                opcoes,
                patrimonioEstimado,
                caixaDisponivel,
                null,
                descontosFiscais,
                null,
                valorizacaoAtivos,
                proventosBrutos,
                irProventos,
                opcoesBrutas,
                irEstimadoOpcoes,
                resultadoRealizadoAcoes);
        }

        private sealed class ValorPatrimonialAtivoRepositoryVazio
            : IValorPatrimonialAtivoRepository
        {
            public Task<IReadOnlyDictionary<string, decimal>>
                ObterUltimosPorTickerAsync(
                    Guid investidorId,
                    CancellationToken cancellationToken = default)
            {
                IReadOnlyDictionary<string, decimal> vazio =
                    new Dictionary<string, decimal>(
                        StringComparer.OrdinalIgnoreCase);

                return Task.FromResult(vazio);
            }
        }

        private static bool EhAtivoSemMarcacaoPorCotacao(
            PosicaoAtivoDto posicao)
        {
            if (posicao.TipoAtivoCodigo ==
                "PREVIDENCIA")
            {
                return true;
            }

            return posicao.Ticker
                .Trim()
                .ToUpperInvariant() switch
            {
                "CDB NEON" => true,
                "CDB BTG" => true,
                "FMP ELETROBRAS" => true,
                _ => false
            };
        }
    }
}