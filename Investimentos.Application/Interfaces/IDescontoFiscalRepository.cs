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

        Task<DescontoFiscal?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        void Excluir(
            DescontoFiscal desconto);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default);

        Task<decimal> ObterTotalAsync(
            CancellationToken cancellationToken = default);
    }
}