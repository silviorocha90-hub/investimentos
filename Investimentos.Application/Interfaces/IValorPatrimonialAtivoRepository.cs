namespace Investimentos.Application.Interfaces
{
    public interface IValorPatrimonialAtivoRepository
    {
        Task<IReadOnlyDictionary<string, decimal>>
            ObterUltimosPorTickerAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default);
    }
}
