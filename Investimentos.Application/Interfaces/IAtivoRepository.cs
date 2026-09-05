using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IAtivoRepository
    {
        Task AdicionarAsync(
            Ativo ativo,
            CancellationToken cancellationToken = default);

        Task<bool> ExistePorTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default);

        Task<TipoAtivo?> ObterTipoAtivoPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default);
    }
}