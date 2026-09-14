using Investimentos.Application.Dashboard;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IHistoricoPatrimonioRepository
    {
        Task<decimal> ObterTotalAtualAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EvolucaoInvestidorDto>>
            ListarEvolucaoAsync(
                CancellationToken cancellationToken = default);

        Task<HistoricoPatrimonio?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task AdicionarAsync(
            HistoricoPatrimonio historicoPatrimonio,
            CancellationToken cancellationToken = default);

        void Excluir(
            HistoricoPatrimonio historicoPatrimonio);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default);
    }
}