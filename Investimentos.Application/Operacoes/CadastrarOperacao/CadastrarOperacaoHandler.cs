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

            if (string.IsNullOrWhiteSpace(
                command.Ticker))
            {
                throw new ArgumentException(
                    "O ticker do ativo é obrigatório.");
            }

            var ativo =
                await _repository.ObterAtivoPorTickerAsync(
                    command.Ticker
                        .Trim()
                        .ToUpperInvariant(),
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

            var tipoAtivoCodigo =
                ativo.TipoAtivo.Codigo
                    .Trim()
                    .ToUpperInvariant();

            var ticker =
                ativo.Ticker.Codigo
                    .Trim()
                    .ToUpperInvariant();

            /*
             * Outros investimentos são controlados pelo valor
             * patrimonial corrente, e não por quantidade de ações.
             * Portanto a baixa/resgate não deve ser bloqueada pela
             * validação de posição usada para renda variável.
             */
            var usaControlePatrimonial =
                tipoAtivoCodigo is "CDB" or "FMP" or "PREVIDENCIA" ||
                ticker.Contains("CDB") ||
                ticker.Contains("FMP ELETROBRAS");

            if (
                tipoOperacao.Codigo == "VENDA" &&
                !usaControlePatrimonial)
            {
                var quantidadeDisponivel =
                    await _repository
                        .ObterQuantidadeDisponivelAsync(
                            investidor.Id,
                            ativo.Id,
                            command.Data,
                            sequencia,
                            cancellationToken);

                if (quantidadeDisponivel <= 0)
                {
                    throw new InvalidOperationException(
                        $"Não existe posição disponível de " +
                        $"{ativo.Ticker.Codigo} para venda.");
                }

                if (command.Quantidade >
                    quantidadeDisponivel)
                {
                    throw new InvalidOperationException(
                        $"Quantidade de venda de " +
                        $"{ativo.Ticker.Codigo} é maior que " +
                        $"a posição disponível. " +
                        $"Disponível: {quantidadeDisponivel}. " +
                        $"Venda: {command.Quantidade}.");
                }
            }

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