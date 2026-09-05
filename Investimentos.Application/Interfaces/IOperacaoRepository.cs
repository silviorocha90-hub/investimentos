using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IOperacaoRepository
    {
        Task AdicionarAsync(
            Operacao operacao,
            CancellationToken cancellationToken = default);

        Task<Investidor?> ObterInvestidorPorIdAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default);

        Task<Ativo?> ObterAtivoPorTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default);

        Task<TipoOperacao?> ObterTipoOperacaoPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default);

        Task<int> ObterProximaSequenciaAsync(
            Guid investidorId,
            DateTime data,
            CancellationToken cancellationToken = default);

        Task<decimal> ObterQuantidadeDisponivelAsync(
            Guid investidorId,
            Guid ativoId,
            DateTime data,
            int sequencia,
            CancellationToken cancellationToken = default);
    }
}