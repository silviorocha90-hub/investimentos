using Investimentos.Application.Carteira.ConsultarCarteira;

namespace Investimentos.Application.Interfaces
{
    public interface ICarteiraRepository
    {
        Task<IReadOnlyList<OperacaoCarteiraDto>>
            ObterOperacoesAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default);
    }
}