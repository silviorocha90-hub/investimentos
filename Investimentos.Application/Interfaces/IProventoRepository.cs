using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IProventoRepository
    {
        Task AdicionarAsync(
            Provento provento,
            CancellationToken cancellationToken = default);

        Task<Investidor?> ObterInvestidorAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default);

        Task<Ativo?> ObterAtivoPorTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Provento>>
            ListarAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default);
    }
}