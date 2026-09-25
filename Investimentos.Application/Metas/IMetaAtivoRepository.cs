namespace Investimentos.Application.Metas
{
    public interface IMetaAtivoRepository
    {
        Task<IReadOnlyList<MetaAtivoDto>> ListarAsync(
            Guid? investidorId = null,
            CancellationToken cancellationToken = default);

        Task<MetaAtivoDto> SalvarAsync(
            SalvarMetaAtivoRequest request,
            CancellationToken cancellationToken = default);

        Task<bool> ExcluirAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
