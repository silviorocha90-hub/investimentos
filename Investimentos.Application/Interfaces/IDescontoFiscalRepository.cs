using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IDescontoFiscalRepository
    {
        Task AdicionarAsync(
            DescontoFiscal desconto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DescontoFiscal>> ListarAsync(
            CancellationToken cancellationToken = default);

        Task<decimal> ObterTotalAsync(
            CancellationToken cancellationToken = default);
    }
}