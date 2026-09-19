namespace Investimentos.Application.Administracao
{
    public interface IAdministracaoRepository
    {
        Task<AdministracaoDto> ObterAsync(
            CancellationToken cancellationToken = default);

        Task<AtivoAdministracaoDto> CriarAtivoAsync(
            CriarAtivoAdministracaoRequest request,
            CancellationToken cancellationToken = default);

        Task<AtivoAdministracaoDto?> AtualizarAtivoAsync(
            Guid ativoId,
            AtualizarAtivoAdministracaoRequest request,
            CancellationToken cancellationToken = default);

        Task<bool> ExcluirAtivoAsync(
            Guid ativoId,
            CancellationToken cancellationToken = default);

        Task<bool> AtualizarClasseAtivoAsync(
            int id,
            string codigo,
            string nome,
            bool ativo,
            CancellationToken cancellationToken = default);

        Task<bool> ExcluirClasseAtivoAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> AtualizarTipoAtivoAsync(
            int id,
            string codigo,
            string nome,
            int classeAtivoId,
            bool ativo,
            CancellationToken cancellationToken = default);

        Task<bool> ExcluirTipoAtivoAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> AtualizarTipoOperacaoAsync(
            int id,
            string codigo,
            string nome,
            bool ativo,
            CancellationToken cancellationToken = default);

        Task<bool> ExcluirTipoOperacaoAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}