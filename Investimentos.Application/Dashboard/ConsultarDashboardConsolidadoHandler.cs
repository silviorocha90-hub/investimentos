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
                         * Investimentos sem
                         * marcação por cotação.
                         */
                        if (EhAtivoSemMarcacaoPorCotacao(
                            posicao))
                        {
                            var valorPatrimonialConsolidado =
                                grupo.Sum(x => x.ValorAtual);

                            return posicao with
                            {
                                PrecoAtual = null,
                                ValorAtual = valorPatrimonialConsolidado,
                                Valorizacao =
                                    valorPatrimonialConsolidado -
                                    custoTotal
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

            /*
             * PROVENTOS E OPÇÕES
             */
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
             * Previdência permanece fora deste
             * indicador por compatibilidade com
             * o conceito atual do Painel.
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
                        x.ValorAtual);

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
             * PATRIMÔNIO
             */
            var patrimonioEstimado =
                valorAplicado +
                valorPrevidencia +
                caixaDisponivel;

            /*
             * DISTRIBUIÇÃO
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
             * RESULTADO REALIZADO EM AÇÕES
             *
             * É somado diretamente dos
             * dashboards individuais.
             *
             * Isso é importante porque uma
             * posição totalmente vendida não
             * existe mais em "Posicoes", mas
             * seu resultado realizado continua
             * existindo.
             */
            var resultadoRealizadoAcoes =
                dashboards.Sum(x =>
                    x.ResultadoRealizadoAcoes);

            /*
             * PROVENTOS
             */
            var totalProventos =
                dashboards.Sum(x =>
                    x.TotalProventos);

            var proventosBrutos =
                dashboards.Sum(x =>
                    x.ProventosBrutos);

            var irProventos =
                dashboards.Sum(x =>
                    x.IrProventos);

            /*
             * OPÇÕES
             *
             * OpcoesBrutas representa o
             * resultado operacional efetivo
             * das operações finalizadas.
             */
            var opcoesBrutas =
                dashboards.Sum(x =>
                    x.OpcoesBrutas);

            /*
             * Mantemos o IR estimado apenas
             * para consulta.
             *
             * Ele NÃO é descontado do resultado
             * da carteira.
             */
            var irEstimadoOpcoes =
                dashboards.Sum(x =>
                    x.IrEstimadoOpcoes);

            /*
             * DESCONTOS FISCAIS
             *
             * Valores efetivamente registrados.
             *
             * São globais e descontados
             * exatamente uma vez.
             */
            var descontosFiscais =
                await _descontoRepository
                    .ObterTotalAsync(
                        cancellationToken);

            /*
             * RESULTADO DE OPÇÕES APÓS
             * DESCONTOS EFETIVOS.
             *
             * A propriedade continua chamada
             * PremioLiquidoOpcoes para manter
             * compatibilidade com o frontend.
             */
            var premioLiquidoOpcoes =
                opcoesBrutas -
                descontosFiscais;

            /*
             * RESULTADO ECONÔMICO DA CARTEIRA
             *
             * Valorização não realizada
             * + resultado realizado em ações
             * + proventos líquidos
             * + opções após descontos fiscais
             *   efetivamente registrados.
             */
            var resultadoCarteira =
                valorizacaoAtivos +
                resultadoRealizadoAcoes +
                totalProventos +
                premioLiquidoOpcoes;

            var entradasAno =
                dashboards.Sum(x =>
                    x.EntradasAno);

            var saidasAno =
                dashboards.Sum(x =>
                    x.SaidasAno);

            var capitalLiquidoAno =
                entradasAno - saidasAno;

            var rentabilidadeAno =
                capitalLiquidoAno > 0
                    ? resultadoCarteira /
                      capitalLiquidoAno * 100
                    : 0;

            return new DashboardDto(
                valorAplicado,
                resultadoCarteira,
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
                valorizacaoAtivos,
                proventosBrutos,
                irProventos,
                opcoesBrutas,
                irEstimadoOpcoes,
                resultadoRealizadoAcoes,
                rentabilidadeAno,
                entradasAno,
                saidasAno);
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