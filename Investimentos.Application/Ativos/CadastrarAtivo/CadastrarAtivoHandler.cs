using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Application.Ativos.CadastrarAtivo
{
    public class CadastrarAtivoHandler
    {
        private readonly IAtivoRepository _repository;

        public CadastrarAtivoHandler(
            IAtivoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(
            CadastrarAtivoCommand command,
            CancellationToken cancellationToken = default)
        {
            var ticker = new Ticker(command.Ticker);

            var existe = await _repository.ExistePorTickerAsync(
                ticker.Codigo,
                cancellationToken);

            if (existe)
            {
                throw new InvalidOperationException(
                    "Já existe um ativo com esse ticker.");
            }

            if (string.IsNullOrWhiteSpace(command.TipoAtivoCodigo))
            {
                throw new ArgumentException(
                    "O tipo do ativo é obrigatório.");
            }

            var tipoAtivo =
                await _repository.ObterTipoAtivoPorCodigoAsync(
                    command.TipoAtivoCodigo
                        .Trim()
                        .ToUpperInvariant(),
                    cancellationToken);

            if (tipoAtivo is null)
            {
                throw new ArgumentException(
                    "O tipo do ativo informado não existe.");
            }

            var ativo = new Ativo(
                ticker,
                command.Nome,
                tipoAtivo);

            await _repository.AdicionarAsync(
                ativo,
                cancellationToken);

            return ativo.Id;
        }
    }
}