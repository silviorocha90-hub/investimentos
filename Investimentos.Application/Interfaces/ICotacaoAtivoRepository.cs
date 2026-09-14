namespace Investimentos.Application.Interfaces
{
    public interface ICotacaoAtivoRepository
    {
        Task<IReadOnlyDictionary<string, decimal>>
            ObterUltimasPorTickerAsync(
                CancellationToken cancellationToken = default);
    }
}