using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Operacoes.ExcluirOperacao
{
    public class ExcluirOperacaoHandler
    {
        private readonly IOperacaoRepository _repository;

        public ExcluirOperacaoHandler(
            IOperacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "A operação é obrigatória.");
            }

            var operacao =
                await _repository.ObterPorIdAsync(
                    id,
                    cancellationToken);

            if (operacao is null)
            {
                throw new InvalidOperationException(
                    "Operação não encontrada.");
            }

            var historicoValido =
                await _repository.HistoricoPermaneceValidoSemOperacaoAsync(
                    operacao.Id,
                    operacao.InvestidorId,
                    operacao.AtivoId,
                    cancellationToken);

            if (!historicoValido)
            {
                throw new InvalidOperationException(
                    "A operação não pode ser excluída porque isso tornaria o histórico da posição inválido.");
            }

            _repository.Remover(operacao);

            await _repository.SalvarAsync(
                cancellationToken);
        }
    }
}