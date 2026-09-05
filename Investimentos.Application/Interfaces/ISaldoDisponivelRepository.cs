namespace Investimentos.Application.Interfaces
{
    public interface ISaldoDisponivelRepository
    {
        Task<decimal> ObterTotalAtualAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Investimentos.Application.Dashboard.SaldoInvestidorDto>>
            ListarAtuaisAsync(
                CancellationToken cancellationToken = default);
    }
}
