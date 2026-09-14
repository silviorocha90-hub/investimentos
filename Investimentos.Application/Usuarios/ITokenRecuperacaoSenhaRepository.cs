using Investimentos.Domain.Usuarios;

namespace Investimentos.Application.Interfaces
{
    public interface ITokenRecuperacaoSenhaRepository
    {
        Task<TokenRecuperacaoSenha?> ObterPorHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default);

        Task AdicionarAsync(
            TokenRecuperacaoSenha token,
            CancellationToken cancellationToken = default);

        Task InvalidarTokensAtivosAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default);
    }
}