using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Performance;

public class ConsultarPerformanceCarteiraService
{
    private readonly IHistoricoPatrimonioRepository _historicoRepository;
    private readonly IMovimentacaoFinanceiraRepository _movimentacaoRepository;

    public ConsultarPerformanceCarteiraService(
        IHistoricoPatrimonioRepository historicoRepository,
        IMovimentacaoFinanceiraRepository movimentacaoRepository)
    {
        _historicoRepository = historicoRepository;
        _movimentacaoRepository = movimentacaoRepository;
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
            return CalculadoraPerformanceCarteira.Calcular(
                historicos,
                movimentacoes);
        }

        var porInvestidor = historicos
            .GroupBy(x => x.InvestidorId)
            .ToList();

        var inicioComum = porInvestidor.Count > 0
            ? porInvestidor.Max(grupo =>
                grupo.Min(x => x.DataReferencia.Date))
            : DateTime.MaxValue;

        var datas = historicos
            .Select(x => x.DataReferencia.Date)
            .Where(x => x >= inicioComum)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var historicoConsolidado = new List<HistoricoPatrimonio>();

        foreach (var data in datas)
        {
            var valoresDisponiveis = porInvestidor
                .Select(grupo => grupo
                    .Where(x => x.DataReferencia.Date <= data)
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

        return CalculadoraPerformanceCarteira.Calcular(
            historicoConsolidado,
            movimentacoes);
    }
}
