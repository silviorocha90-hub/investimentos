namespace Investimentos.Application.Administracao.Integridade
{
    public interface IIntegridadeRepository
    {
        Task<IntegridadeDto> VerificarAsync(
            CancellationToken cancellationToken = default);
    }
}
