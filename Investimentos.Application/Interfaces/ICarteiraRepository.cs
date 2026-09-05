using Investimentos.Application.Carteira.ConsultarCarteira;

namespace Investimentos.Application.Interfaces
{
    public interface ICarteiraRepository
    {
        Task<IReadOnlyList<PosicaoAtivoDto>> ObterPosicoesAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default);
    }
}