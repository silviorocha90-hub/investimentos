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

        public ConsultarDashboardHandler(
            ConsultarCarteiraHandler carteiraHandler,
            ConsultarProventosHandler proventosHandler,
            ConsultarOpcoesHandler opcoesHandler,
            ICotacaoAtivoRepository cotacaoRepository,
            ISaldoDisponivelRepository saldoRepository)
        {
            _carteiraHandler = carteiraHandler;
            _proventosHandler = proventosHandler;
            _opcoesHandler = opcoesHandler;
            _cotacaoRepository = cotacaoRepository;
            _saldoRepository = saldoRepository;
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

            var posicoes =
                posicoesOriginais
                    .Select(x =>
                    {
                        if (EhAtivoSemMarcacaoPorCotacao(x))
                        {
                            return x with
                            {
                                PrecoAtual = null,
                                ValorAtual = x.CustoTotal,
                                Valorizacao = 0
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

                        var valorAtual =
                            x.Quantidade *
                            precoAtual;

                        return x with
                        {
                            PrecoAtual = precoAtual,
                            ValorAtual = valorAtual,
                            Valorizacao =
                                valorAtual -
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
                        x.CustoTotal);

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
             *
             * O valor recebido já é líquido da
             * retenção registrada no lançamento.
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
             * OPÇÕES
             *
             * ResultadoBruto representa o
             * resultado operacional.
             *
             * IrEstimado é apenas uma estimativa
             * por operação. A apuração fiscal
             * mensal e compensação de prejuízos
             * serão tratadas posteriormente.
             */
            var opcoesFinalizadas =
                opcoes
                    .Where(x =>
                        x.Situacao == "ENCERRADA" ||
                        x.Situacao == "EXECUTADA" ||
                        x.Situacao == "EXPIRADA")
                    .ToList();

            var opcoesBrutas =
                opcoesFinalizadas.Sum(x =>
                    x.ResultadoBruto ?? 0);

            var irEstimadoOpcoes =
                opcoesFinalizadas.Sum(x =>
                    x.IrEstimado);

            var premioLiquidoOpcoes =
                opcoesFinalizadas.Sum(x =>
                    x.ResultadoLiquido ?? 0);

            var valorizacaoAtivos =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        !EhAtivoSemMarcacaoPorCotacao(x) &&
                        x.PrecoAtual.HasValue)
                    .Sum(x =>
                        x.Valorizacao);

            var resultadoRealizado =
                valorizacaoAtivos +
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
                resultadoRealizado,
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
                irEstimadoOpcoes);
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