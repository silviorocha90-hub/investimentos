using Investimentos.Domain.Entities;

namespace Investimentos.Application.Administracao
{
    public interface IInvestidorAdministracaoRepository
    {
        Task<Investidor?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default);
    }
}