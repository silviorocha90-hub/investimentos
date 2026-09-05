using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IInvestidorRepository
    {
        Task AdicionarAsync(
            Investidor investidor,
            CancellationToken cancellationToken = default);

        Task<bool> ExistePorNomeAsync(
            string nome,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Investidor>> ListarAsync(
            CancellationToken cancellationToken = default);
    }
}