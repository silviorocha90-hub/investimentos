namespace Investimentos.Application.Fiscal
{
    public interface IFiscalRepository
    {
        Task<FiscalResumoDto> ConsultarAsync(
            Guid? investidorId = null,
            int? ano = null,
            CancellationToken cancellationToken = default);
    }
}
