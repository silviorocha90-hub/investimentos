using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Opcoes.CadastrarOperacaoOpcao
{
    public class CadastrarOperacaoOpcaoHandler
    {
        private readonly IOperacaoOpcaoRepository _repository;

        public CadastrarOperacaoOpcaoHandler(
            IOperacaoOpcaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(
            CadastrarOperacaoOpcaoCommand command,
            CancellationToken cancellationToken = default)
        {
            var investidor =
                await _repository.ObterInvestidorAsync(
                    command.InvestidorId,
                    cancellationToken);

            if (investidor is null)
                throw new ArgumentException(
                    "O investidor informado não existe.");

            if (string.IsNullOrWhiteSpace(command.TickerAtivo))
                throw new ArgumentException(
                    "O ticker do ativo objeto é obrigatório.");

            var ativo =
                await _repository.ObterAtivoPorTickerAsync(
                    command.TickerAtivo
                        .Trim()
                        .ToUpperInvariant(),
                    cancellationToken);

            if (ativo is null)
                throw new ArgumentException(
                    "O ativo objeto informado não existe.");

            var operacao =
                new OperacaoOpcao(
                    investidor,
                    ativo,
                    command.TickerOpcao,
                    command.TipoOpcao,
                    command.Natureza,
                    command.DataOperacao,
                    command.Vencimento,
                    command.Strike,
                    command.Contratos,
                    command.Quantidade,
                    command.PremioUnitario,
                    command.Taxas);

            await _repository.AdicionarAsync(
                operacao,
                cancellationToken);

            return operacao.Id;
        }
    }
}