using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Operacoes.CadastrarOperacao
{
    public class CadastrarOperacaoHandler
    {
        private readonly IOperacaoRepository _repository;

        public CadastrarOperacaoHandler(
            IOperacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(
            CadastrarOperacaoCommand command,
            CancellationToken cancellationToken = default)
        {
            var investidor =
                await _repository.ObterInvestidorPorIdAsync(
                    command.InvestidorId,
                    cancellationToken);

            if (investidor is null)
            {
                throw new ArgumentException(
                    "O investidor informado não existe.");
            }

            if (string.IsNullOrWhiteSpace(command.Ticker))
            {
                throw new ArgumentException(
                    "O ticker do ativo é obrigatório.");
            }

            var ticker =
                command.Ticker
                    .Trim()
                    .ToUpperInvariant();

            var ativo =
                await _repository.ObterAtivoPorTickerAsync(
                    ticker,
                    cancellationToken);

            if (ativo is null)
            {
                throw new ArgumentException(
                    "O ativo informado não existe.");
            }

            if (string.IsNullOrWhiteSpace(
                command.TipoOperacaoCodigo))
            {
                throw new ArgumentException(
                    "O tipo da operação é obrigatório.");
            }

            var tipoOperacao =
                await _repository.ObterTipoOperacaoPorCodigoAsync(
                    command.TipoOperacaoCodigo
                        .Trim()
                        .ToUpperInvariant(),
                    cancellationToken);

            if (tipoOperacao is null)
            {
                throw new ArgumentException(
                    "O tipo da operação informado não existe.");
            }

            var sequencia =
                await _repository.ObterProximaSequenciaAsync(
                    investidor.Id,
                    command.Data,
                    cancellationToken);

            var operacao =
                new Operacao(
                    command.Data,
                    sequencia,
                    investidor,
                    ativo,
                    tipoOperacao,
                    command.Quantidade,
                    command.PrecoUnitario,
                    command.Taxas);

            await _repository.AdicionarAsync(
                operacao,
                cancellationToken);

            return operacao.Id;
        }
    }
}