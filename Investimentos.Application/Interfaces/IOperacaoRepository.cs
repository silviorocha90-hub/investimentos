using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IOperacaoRepository
    {
        Task AdicionarAsync(
            Operacao operacao,
            CancellationToken cancellationToken = default);

        Task<Operacao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        void Remover(
            Operacao operacao);

        Task SalvarAsync(
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

        Task<bool> HistoricoPermaneceValidoSemOperacaoAsync(
            Guid operacaoId,
            Guid investidorId,
            Guid ativoId,
            CancellationToken cancellationToken = default);
    }
}