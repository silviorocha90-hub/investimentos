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

            /*
             * ENRIQUECIMENTO DAS POSIÇÕES
             *
             * Ativos com cotação:
             *
             * Valor Atual =
             * Quantidade x Preço Atual
             *
             * Valorização =
             * Valor Atual - Custo Total
             *
             * Ativos patrimoniais sem marcação
             * por cotação:
             *
             * - PREVIDENCIA
             * - CDB NEON
             * - CDB BTG
             * - FMP ELETROBRAS
             *
             * Para esses ativos:
             *
             * Preço Atual = null
             * Valor Atual = CustoTotal
             * Valorização = 0
             */
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

                        var valorizacao =
                            valorAtual -
                            x.CustoTotal;

                        return x with
                        {
                            PrecoAtual = precoAtual,
                            ValorAtual = valorAtual,
                            Valorizacao = valorizacao
                        };
                    })
                    .ToList();

            /*
             * VALOR APLICADO
             *
             * Inclui todos os investimentos
             * atuais, exceto Previdência.
             *
             * CDB NEON, CDB BTG e
             * FMP ELETROBRAS continuam sendo
             * considerados no Valor Aplicado.
             */
            var valorAplicado =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        x.TipoAtivoCodigo !=
                            "PREVIDENCIA")
                    .Sum(x =>
                        x.ValorAtual);

            /*
             * PREVIDÊNCIA
             *
             * Continua separada do Valor Aplicado
             * e entra diretamente no patrimônio.
             */
            var valorPrevidencia =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        x.TipoAtivoCodigo ==
                            "PREVIDENCIA")
                    .Sum(x =>
                        x.CustoTotal);

            /*
             * CAIXA
             */
            var saldoAtual =
                await _saldoRepository
                    .ObterAtualAsync(
                        investidorId,
                        cancellationToken);

            var caixaDisponivel =
                saldoAtual?.Valor ?? 0;

            /*
             * PATRIMÔNIO ATUAL
             *
             * Não somamos novamente proventos,
             * opções ou valorização.
             *
             * Eles já estão refletidos no valor
             * atual dos ativos e/ou no caixa.
             */
            var patrimonioEstimado =
                valorAplicado +
                valorPrevidencia +
                caixaDisponivel;

            /*
             * PROVENTOS
             */
            var totalProventos =
                proventos.Sum(
                    x => x.ValorRecebido);

            /*
             * OPÇÕES
             *
             * Consideramos somente operações
             * finalizadas.
             */
            var premioLiquidoOpcoes =
                opcoes
                    .Where(x =>
                        x.Situacao == "ENCERRADA" ||
                        x.Situacao == "EXECUTADA")
                    .Sum(x =>
                        x.ResultadoInformado ??
                        x.ResultadoFinal ??
                        0);

            /*
             * VALORIZAÇÃO DOS ATIVOS
             *
             * Entram somente posições que possuem
             * marcação por cotação.
             *
             * PREVIDENCIA, CDB NEON, CDB BTG e
             * FMP ELETROBRAS não participam.
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
             * RESULTADO TOTAL DA CARTEIRA
             *
             * Resultado =
             * Valorização dos ativos
             * + Proventos
             * + Opções
             */
            var resultadoRealizado =
                valorizacaoAtivos +
                totalProventos +
                premioLiquidoOpcoes;

            /*
             * Descontos fiscais são globais.
             * Portanto não são descontados
             * no dashboard individual.
             */
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
                valorizacaoAtivos);
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