using Investimentos.Application.Interfaces;
using Investimentos.Application.Carteira.ConsultarCarteira;

namespace Investimentos.Application.Dashboard
{
    public class ConsultarDashboardConsolidadoHandler
    {
        private readonly IInvestidorRepository _investidorRepository;
        private readonly ConsultarDashboardHandler _dashboardHandler;
        private readonly IHistoricoPatrimonioRepository _historicoRepository;
        private readonly ISaldoDisponivelRepository _saldoRepository;
        private readonly ICotacaoAtivoRepository _cotacaoRepository;

        public ConsultarDashboardConsolidadoHandler(
            IInvestidorRepository investidorRepository,
            ConsultarDashboardHandler dashboardHandler,
            IHistoricoPatrimonioRepository historicoRepository,
            ISaldoDisponivelRepository saldoRepository,
            ICotacaoAtivoRepository cotacaoRepository)
        {
            _investidorRepository = investidorRepository;
            _dashboardHandler = dashboardHandler;
            _historicoRepository = historicoRepository;
            _saldoRepository = saldoRepository;
            _cotacaoRepository = cotacaoRepository;
        }

        public async Task<DashboardDto> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            var investidores =
                await _investidorRepository.ListarAsync(
                    cancellationToken);

            var dashboards = new List<DashboardDto>();

            foreach (var investidor in investidores)
            {
                dashboards.Add(
                    await _dashboardHandler.HandleAsync(
                        investidor.Id,
                        cancellationToken));
            }

            var posicoes = dashboards
                .SelectMany(x => x.Posicoes)
                .GroupBy(x => new { x.Ticker, x.Nome })
                .Select(grupo =>
                {
                    var quantidade = grupo.Sum(x => x.Quantidade);
                    var custoTotal = grupo.Sum(x => x.CustoTotal);

                    return new PosicaoAtivoDto(
                        grupo.Key.Ticker,
                        grupo.Key.Nome,
                        quantidade,
                        quantidade > 0 ? custoTotal / quantidade : 0,
                        custoTotal,
                        grupo.Sum(x => x.ResultadoRealizado),
                        grupo.Min(x => x.DataPrimeiraCompra));
                })
                .Where(x =>
                    x.Quantidade > 0)
                .OrderBy(x => x.Ticker)
                .ToList();

            var proventos = dashboards
                .SelectMany(x => x.Proventos)
                .OrderByDescending(x => x.DataPagamento)
                .ToList();

            var opcoes = dashboards
                .SelectMany(x => x.Opcoes)
                .OrderByDescending(x => x.DataOperacao)
                .ToList();

            var cotacoes = await _cotacaoRepository
                .ObterUltimasPorTickerAsync(cancellationToken);

            var valorAplicado = posicoes
                .Where(x =>
                    x.Quantidade > 0 &&
                    x.Ticker != "PREV")
                .Sum(x => x.Quantidade *
                    (cotacoes.TryGetValue(x.Ticker, out var preco)
                        ? preco
                        : 0));

            return new DashboardDto(
                valorAplicado,
                dashboards.Sum(x => x.ResultadoRealizado),
                dashboards.Sum(x => x.TotalProventos),
                dashboards.Sum(x => x.PremioLiquidoOpcoes),
                posicoes.Count(x => x.Quantidade > 0),
                opcoes.Count(x => x.Situacao == "ABERTA"),
                posicoes,
                proventos,
                opcoes,
                await _historicoRepository.ObterTotalAtualAsync(cancellationToken),
                await _saldoRepository.ObterTotalAtualAsync(cancellationToken),
                await _saldoRepository.ListarAtuaisAsync(cancellationToken),
                dashboards.Sum(x => x.DescontosFiscais));
        }
    }
}
