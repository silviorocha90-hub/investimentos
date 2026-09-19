namespace Investimentos.Application.Interfaces
{
    public interface IValorPatrimonialAtivoRepository
    {
        Task<IReadOnlyDictionary<string, decimal>>
            ObterUltimosPorTickerAsync(
                CancellationToken cancellationToken = default);
    }
}
