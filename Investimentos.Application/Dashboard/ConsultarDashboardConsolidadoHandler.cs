using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Dashboard
{
    public class ConsultarDashboardConsolidadoHandler
    {
        private readonly IInvestidorRepository _investidorRepository;
        private readonly ConsultarDashboardHandler _dashboardHandler;
        private readonly ISaldoDisponivelRepository _saldoRepository;
        private readonly ICotacaoAtivoRepository _cotacaoRepository;
        private readonly IDescontoFiscalRepository _descontoRepository;

        public ConsultarDashboardConsolidadoHandler(
            IInvestidorRepository investidorRepository,
            ConsultarDashboardHandler dashboardHandler,
            ISaldoDisponivelRepository saldoRepository,
            ICotacaoAtivoRepository cotacaoRepository,
            IDescontoFiscalRepository descontoRepository)
        {
            _investidorRepository = investidorRepository;
            _dashboardHandler = dashboardHandler;
            _saldoRepository = saldoRepository;
            _cotacaoRepository = cotacaoRepository;
            _descontoRepository = descontoRepository;
        }

        public async Task<DashboardDto> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            var investidores =
                await _investidorRepository.ListarAsync(
                    cancellationToken);

            var dashboards =
                new List<DashboardDto>();

            foreach (var investidor in investidores)
            {
                dashboards.Add(
                    await _dashboardHandler.HandleAsync(
                        investidor.Id,
                        cancellationToken));
            }

            var cotacoes =
                await _cotacaoRepository
                    .ObterUltimasPorTickerAsync(
                        cancellationToken);

            /*
             * CONSOLIDAÇÃO DAS POSIÇÕES
             */
            var posicoes =
                dashboards
                    .SelectMany(x =>
                        x.Posicoes)
                    .GroupBy(x => new
                    {
                        x.Ticker,
                        x.Nome,
                        x.TipoAtivoCodigo,
                        x.TipoAtivoNome
                    })
                    .Select(grupo =>
                    {
                        var quantidade =
                            grupo.Sum(x =>
                                x.Quantidade);

                        var custoTotal =
                            grupo.Sum(x =>
                                x.CustoTotal);

                        var posicao =
                            new PosicaoAtivoDto(
                                grupo.Key.Ticker,
                                grupo.Key.Nome,
                                grupo.Key.TipoAtivoCodigo,
                                grupo.Key.TipoAtivoNome,
                                quantidade,
                                quantidade > 0
                                    ? custoTotal /
                                      quantidade
                                    : 0,
                                custoTotal,
                                grupo.Sum(x =>
                                    x.ResultadoRealizado),
                                grupo.Min(x =>
                                    x.DataPrimeiraCompra));

                        /*
                         * Ativos patrimoniais sem
                         * marcação por cotação.
                         *
                         * Valor Atual = Custo Total
                         * Valorização = 0
                         */
                        if (EhAtivoSemMarcacaoPorCotacao(
                            posicao))
                        {
                            return posicao with
                            {
                                PrecoAtual = null,
                                ValorAtual = custoTotal,
                                Valorizacao = 0
                            };
                        }

                        if (!cotacoes.TryGetValue(
                            grupo.Key.Ticker,
                            out var precoAtual))
                        {
                            return posicao with
                            {
                                PrecoAtual = null,
                                ValorAtual = 0,
                                Valorizacao = 0
                            };
                        }

                        var valorAtual =
                            quantidade *
                            precoAtual;

                        return posicao with
                        {
                            PrecoAtual = precoAtual,
                            ValorAtual = valorAtual,
                            Valorizacao =
                                valorAtual -
                                custoTotal
                        };
                    })
                    .Where(x =>
                        x.Quantidade > 0)
                    .OrderBy(x =>
                        x.Ticker)
                    .ToList();

            var proventos =
                dashboards
                    .SelectMany(x =>
                        x.Proventos)
                    .OrderByDescending(x =>
                        x.DataPagamento)
                    .ToList();

            var opcoes =
                dashboards
                    .SelectMany(x =>
                        x.Opcoes)
                    .OrderByDescending(x =>
                        x.DataOperacao)
                    .ToList();

            /*
             * VALOR APLICADO
             *
             * Não inclui Previdência.
             *
             * CDB NEON, CDB BTG e
             * FMP ELETROBRAS continuam
             * incluídos.
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
            var caixaDisponivel =
                await _saldoRepository
                    .ObterTotalAtualAsync(
                        cancellationToken);

            var saldosDisponiveis =
                await _saldoRepository
                    .ListarAtuaisAsync(
                        cancellationToken);

            /*
             * PATRIMÔNIO ATUAL
             */
            var patrimonioEstimado =
                valorAplicado +
                valorPrevidencia +
                caixaDisponivel;

            /*
             * DISTRIBUIÇÃO DA CARTEIRA
             *
             * Inclui todos os investimentos
             * com valor patrimonial atual.
             *
             * O caixa é acrescentado pelo
             * frontend como "Disponível".
             */
            var valoresPorTipo =
                posicoes
                    .Where(x =>
                        x.Quantidade > 0 &&
                        x.ValorAtual > 0)
                    .GroupBy(x => new
                    {
                        x.TipoAtivoCodigo,
                        x.TipoAtivoNome
                    })
                    .Select(grupo => new
                    {
                        Codigo =
                            grupo.Key.TipoAtivoCodigo,

                        Nome =
                            grupo.Key.TipoAtivoNome,

                        Valor =
                            grupo.Sum(x =>
                                x.ValorAtual)
                    })
                    .OrderByDescending(x =>
                        x.Valor)
                    .ToList();

            var totalDistribuicao =
                valoresPorTipo.Sum(x =>
                    x.Valor);

            var distribuicaoPorTipo =
                valoresPorTipo
                    .Select(x =>
                        new DistribuicaoTipoAtivoDto(
                            x.Codigo,
                            x.Nome,
                            x.Valor,
                            totalDistribuicao > 0
                                ? x.Valor /
                                  totalDistribuicao *
                                  100
                                : 0))
                    .ToList();

            /*
             * VALORIZAÇÃO DOS ATIVOS
             *
             * Somente posições marcadas
             * por cotação participam.
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
             * PROVENTOS
             */
            var totalProventos =
                dashboards.Sum(x =>
                    x.TotalProventos);

            /*
             * DESCONTOS FISCAIS
             *
             * São globais e são descontados
             * uma única vez no consolidado.
             */
            var descontosFiscais =
                await _descontoRepository
                    .ObterTotalAsync(
                        cancellationToken);

            /*
             * OPÇÕES LÍQUIDAS
             */
            var premioLiquidoOpcoes =
                dashboards.Sum(x =>
                    x.PremioLiquidoOpcoes) -
                descontosFiscais;

            /*
             * RESULTADO TOTAL CONSOLIDADO
             *
             * Valorização
             * + Proventos
             * + Opções líquidas de impostos
             */
            var resultadoRealizado =
                valorizacaoAtivos +
                totalProventos +
                premioLiquidoOpcoes;

            return new DashboardDto(
                valorAplicado,
                resultadoRealizado,
                totalProventos,
                premioLiquidoOpcoes,
                posicoes.Count(x =>
                    x.Quantidade > 0 &&
                    x.TipoAtivoCodigo !=
                        "PREVIDENCIA"),
                opcoes.Count(x =>
                    x.Situacao == "ABERTA"),
                posicoes,
                proventos,
                opcoes,
                patrimonioEstimado,
                caixaDisponivel,
                saldosDisponiveis,
                descontosFiscais,
                distribuicaoPorTipo,
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