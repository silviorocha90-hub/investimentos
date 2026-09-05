using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Operacoes.AtualizarOperacao
{
    public class AtualizarOperacaoHandler
    {
        private readonly IOperacaoRepository _repository;

        public AtualizarOperacaoHandler(
            IOperacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            AtualizarOperacaoCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "A operação é obrigatória.");
            }

            var operacao = await _repository.ObterPorIdAsync(
                command.Id,
                cancellationToken);

            if (operacao is null)
            {
                throw new KeyNotFoundException(
                    "Operação não encontrada.");
            }

            operacao.Atualizar(
                command.Data,
                command.Quantidade,
                command.PrecoUnitario,
                command.Taxas);

            await _repository.SalvarAsync(cancellationToken);
        }
    }
}
