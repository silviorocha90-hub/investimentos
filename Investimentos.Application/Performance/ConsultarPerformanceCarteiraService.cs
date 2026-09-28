using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Performance;

public class ConsultarPerformanceCarteiraService
{
    private readonly IHistoricoPatrimonioRepository _historicoRepository;
    private readonly IMovimentacaoFinanceiraRepository _movimentacaoRepository;
    private readonly ConsultarDashboardHandler _dashboardHandler;
    private readonly ConsultarDashboardConsolidadoHandler _dashboardConsolidadoHandler;

    public ConsultarPerformanceCarteiraService(
        IHistoricoPatrimonioRepository historicoRepository,
        IMovimentacaoFinanceiraRepository movimentacaoRepository,
        ConsultarDashboardHandler dashboardHandler,
        ConsultarDashboardConsolidadoHandler dashboardConsolidadoHandler)
    {
        _historicoRepository = historicoRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _dashboardHandler = dashboardHandler;
        _dashboardConsolidadoHandler = dashboardConsolidadoHandler;
    }

    public async Task<PerformanceCarteiraDto> ConsultarAsync(
        Guid? investidorId,
        CancellationToken cancellationToken = default)
    {
        var historicos = await _historicoRepository
            .ListarAsync(investidorId, cancellationToken);

        var movimentacoes = await _movimentacaoRepository
            .ListarAsync(investidorId, cancellationToken);

        if (investidorId.HasValue)
        {
            var dashboardAtual = await _dashboardHandler.HandleAsync(
                investidorId.Value,
                cancellationToken);

            var historicosComAtual =
                IncluirPatrimonioAtual(
                    historicos,
                    dashboardAtual.PatrimonioEstimado);

            return CalculadoraPerformanceCarteira.Calcular(
                historicosComAtual,
                movimentacoes);
        }

        var porInvestidor = historicos
            .GroupBy(x => x.InvestidorId)
            .ToList();

        var inicioComum = porInvestidor.Count > 0
            ? porInvestidor.Max(grupo =>
                grupo.Min(x => x.DataReferencia.Date))
            : DateTime.MaxValue;

        // No consolidado, um ponto patrimonial só é comparável quando
        // todos os investidores possuem snapshot exatamente na mesma data.
        // Usar o último valor anterior de cada investidor mistura competências
        // diferentes e pode transformar atualização de cadastro em rentabilidade.
        var datas = porInvestidor.Count > 0
            ? porInvestidor
                .Select(grupo => grupo
                    .Select(x => x.DataReferencia.Date)
                    .Where(x => x >= inicioComum)
                    .Distinct())
                .Aggregate((comuns, proximas) =>
                    comuns.Intersect(proximas))
                .OrderBy(x => x)
                .ToList()
            : new List<DateTime>();

        var historicoConsolidado = new List<HistoricoPatrimonio>();

        foreach (var data in datas)
        {
            var valoresDisponiveis = porInvestidor
                .Select(grupo => grupo
                    .Where(x => x.DataReferencia.Date == data)
                    .OrderByDescending(x => x.DataReferencia)
                    .FirstOrDefault())
                .Where(x => x is not null)
                .ToList();

            if (valoresDisponiveis.Count == 0)
            {
                continue;
            }

            var referencia = valoresDisponiveis[0]!;
            historicoConsolidado.Add(
                new HistoricoPatrimonio(
                    referencia.Investidor,
                    data,
                    valoresDisponiveis.Sum(x => x!.ValorCarteira)));
        }

        var dashboardConsolidado =
            await _dashboardConsolidadoHandler.HandleAsync(
                cancellationToken);

        var consolidadoComAtual =
            IncluirPatrimonioAtual(
                historicoConsolidado,
                dashboardConsolidado.PatrimonioEstimado);

        return CalculadoraPerformanceCarteira.Calcular(
            consolidadoComAtual,
            movimentacoes);
    }

    private static IReadOnlyList<HistoricoPatrimonio>
        IncluirPatrimonioAtual(
            IEnumerable<HistoricoPatrimonio> historicos,
            decimal patrimonioAtual)
    {
        var pontos = historicos
            .OrderBy(x => x.DataReferencia)
            .ToList();

        if (pontos.Count == 0)
        {
            return pontos;
        }

        var hoje = DateTime.Today;
        var ultimo = pontos[^1];

        if (ultimo.DataReferencia.Date == hoje)
        {
            pontos[^1] = new HistoricoPatrimonio(
                ultimo.Investidor,
                hoje,
                patrimonioAtual);

            return pontos;
        }

        if (ultimo.DataReferencia.Date < hoje)
        {
            pontos.Add(
                new HistoricoPatrimonio(
                    ultimo.Investidor,
                    hoje,
                    patrimonioAtual));
        }

        return pontos;
    }
}
