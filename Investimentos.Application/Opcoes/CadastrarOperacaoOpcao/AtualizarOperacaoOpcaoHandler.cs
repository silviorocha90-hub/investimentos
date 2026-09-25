using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Opcoes.AtualizarOperacaoOpcao
{
    public record AtualizarOperacaoOpcaoCommand(
        Guid Id,
        DateTime DataOperacao,
        DateTime Vencimento,
        decimal Strike,
        int Contratos,
        decimal Quantidade,
        decimal PremioUnitario,
        decimal Taxas,
        string Situacao,
        DateTime? DataFinalizacao,
        decimal? PrecoRecompraUnitario,
        decimal? ValorExecucao,
        decimal? ResultadoInformado);

    public class AtualizarOperacaoOpcaoHandler
    {
        private readonly IOperacaoOpcaoCrudRepository _repository;

        public AtualizarOperacaoOpcaoHandler(
            IOperacaoOpcaoCrudRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            AtualizarOperacaoOpcaoCommand command,
            CancellationToken cancellationToken)
        {
            var opcao =
                await _repository.ObterPorIdAsync(
                    command.Id,
                    cancellationToken);

            if (opcao is null)
            {
                throw new KeyNotFoundException(
                    "Operação de opção não encontrada.");
            }

            var situacaoAtual =
                opcao.Situacao
                    .Trim()
                    .ToUpperInvariant();

            var novaSituacao =
                command.Situacao
                    .Trim()
                    .ToUpperInvariant();

            if (situacaoAtual == "EXECUTADA")
            {
                if (novaSituacao != "ENCERRADA")
                {
                    throw new InvalidOperationException(
                        "Uma opção executada somente pode ser alterada para ENCERRADA.");
                }

                opcao.AtualizarResultadoInformado(
                    command.ResultadoInformado);

                opcao.AtualizarFinalizacao(
                    "ENCERRADA",
                    command.DataFinalizacao,
                    command.PrecoRecompraUnitario,
                    null);
            }
            else if (situacaoAtual == "ENCERRADA")
            {
                if (novaSituacao != "ENCERRADA")
                {
                    throw new InvalidOperationException(
                        "O status de uma opção encerrada não pode mais ser alterado.");
                }

                opcao.AtualizarResultadoInformado(
                    command.ResultadoInformado);

                opcao.AtualizarFinalizacao(
                    "ENCERRADA",
                    command.DataFinalizacao,
                    command.PrecoRecompraUnitario,
                    null);
            }
            else
            {
                throw new InvalidOperationException(
                    "Somente opções EXECUTADAS ou ENCERRADAS podem ser editadas por esta operação.");
            }

            await _repository.SalvarAlteracoesAsync(
                cancellationToken);
        }
    }
}