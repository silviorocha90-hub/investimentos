using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IMovimentacaoFinanceiraRepository
    {
        Task<IReadOnlyList<MovimentacaoFinanceira>> ListarAsync(
            Guid? investidorId = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MovimentacaoFinanceira>> ListarAnoAsync(
            Guid investidorId,
            int ano,
            CancellationToken cancellationToken = default);
    }
}