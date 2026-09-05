using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Investidores.CadastrarInvestidor
{
    public class CadastrarInvestidorHandler
    {
        private readonly IInvestidorRepository _repository;

        public CadastrarInvestidorHandler(
            IInvestidorRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(
            CadastrarInvestidorCommand command,
            CancellationToken cancellationToken = default)
        {
            var investidor =
                new Investidor(command.Nome);

            var existe =
                await _repository.ExistePorNomeAsync(
                    investidor.Nome,
                    cancellationToken);

            if (existe)
            {
                throw new InvalidOperationException(
                    "Já existe um investidor com esse nome.");
            }

            await _repository.AdicionarAsync(
                investidor,
                cancellationToken);

            return investidor.Id;
        }
    }
}