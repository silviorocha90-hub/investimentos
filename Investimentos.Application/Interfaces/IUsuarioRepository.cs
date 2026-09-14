using Investimentos.Domain.Usuarios;

namespace Investimentos.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Usuario?> ObterPorEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<bool> ExistePorEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Usuario>> ListarAsync(
            CancellationToken cancellationToken = default);

        Task AdicionarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default);
    }
}