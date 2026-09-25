using Investimentos.Application.Interfaces;

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

        var historicoConsolidado = historicos
            .GroupBy(x => x.DataReferencia.Date)
            .Select(grupo =>
            {
                var primeiro = grupo.First();
                return new HistoricoPatrimonio(
                    primeiro.Investidor,
                    grupo.Key,
                    grupo.Sum(x => x.ValorCarteira));
            })
            .OrderBy(x => x.DataReferencia)
            .ToList();

        return CalculadoraPerformanceCarteira.Calcular(
            historicoConsolidado,
            movimentacoes);
    }
}
