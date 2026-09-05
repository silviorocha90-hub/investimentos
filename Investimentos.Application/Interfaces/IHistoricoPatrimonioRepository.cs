namespace Investimentos.Application.Interfaces
{
    public interface IHistoricoPatrimonioRepository
    {
        Task<decimal> ObterTotalAtualAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Investimentos.Application.Dashboard.EvolucaoInvestidorDto>>
            ListarEvolucaoAsync(
                CancellationToken cancellationToken = default);
    }
}
