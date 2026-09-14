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
    }
}