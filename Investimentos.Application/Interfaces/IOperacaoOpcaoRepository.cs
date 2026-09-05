using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IOperacaoOpcaoRepository
    {
        Task AdicionarAsync(
            OperacaoOpcao operacao,
            CancellationToken cancellationToken = default);

        Task<Investidor?> ObterInvestidorAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default);

        Task<Ativo?> ObterAtivoPorTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OperacaoOpcao>>
            ListarAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default);
    }
}