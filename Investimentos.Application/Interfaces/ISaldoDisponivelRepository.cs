using Investimentos.Application.Dashboard;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface ISaldoDisponivelRepository
    {
        Task<decimal> ObterTotalAtualAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SaldoInvestidorDto>>
            ListarAtuaisAsync(
                CancellationToken cancellationToken = default);

        Task<SaldoDisponivel?> ObterAtualAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default);

        Task AdicionarAsync(
            SaldoDisponivel saldoDisponivel,
            CancellationToken cancellationToken = default);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default);
    }
}