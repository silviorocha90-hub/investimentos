using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Proventos.CadastrarProvento
{
    public class CadastrarProventoHandler
    {
        private readonly IProventoRepository _repository;

        public CadastrarProventoHandler(
            IProventoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(
            CadastrarProventoCommand command,
            CancellationToken cancellationToken = default)
        {
            var investidor =
                await _repository.ObterInvestidorAsync(
                    command.InvestidorId,
                    cancellationToken);

            if (investidor is null)
                throw new ArgumentException(
                    "O investidor informado não existe.");

            if (string.IsNullOrWhiteSpace(command.Ticker))
                throw new ArgumentException(
                    "O ticker é obrigatório.");

            var ativo =
                await _repository.ObterAtivoPorTickerAsync(
                    command.Ticker
                        .Trim()
                        .ToUpperInvariant(),
                    cancellationToken);

            if (ativo is null)
                throw new ArgumentException(
                    "O ativo informado não existe.");

            var provento =
                new Provento(
                    investidor,
                    ativo,
                    command.Tipo,
                    command.DataCom,
                    command.DataPagamento,
                    command.QuantidadeBase,
                    command.ValorPorUnidade);

            await _repository.AdicionarAsync(
                provento,
                cancellationToken);

            return provento.Id;
        }
    }
}