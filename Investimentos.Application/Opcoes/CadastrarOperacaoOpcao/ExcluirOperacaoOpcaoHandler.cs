using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Opcoes.ExcluirOperacaoOpcao
{
    public class ExcluirOperacaoOpcaoHandler
    {
        private readonly IOperacaoOpcaoCrudRepository _repository;

        public ExcluirOperacaoOpcaoHandler(
            IOperacaoOpcaoCrudRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var opcao =
                await _repository.ObterPorIdAsync(
                    id,
                    cancellationToken);

            if (opcao is null)
            {
                throw new KeyNotFoundException(
                    "Operação de opção não encontrada.");
            }

            _repository.Excluir(opcao);

            await _repository.SalvarAlteracoesAsync(
                cancellationToken);
        }
    }
}