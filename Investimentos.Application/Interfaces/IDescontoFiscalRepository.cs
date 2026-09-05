using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IDescontoFiscalRepository
    {
        Task AdicionarAsync(DescontoFiscal desconto, CancellationToken cancellationToken = default);
        Task<Investidor?> ObterInvestidorAsync(Guid investidorId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DescontoFiscal>> ListarAsync(Guid? investidorId = null, CancellationToken cancellationToken = default);
        Task<decimal> ObterTotalAsync(Guid investidorId, CancellationToken cancellationToken = default);
    }
}
