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

            opcao.AtualizarDados(
                command.DataOperacao,
                command.Vencimento,
                command.Strike,
                command.Contratos,
                command.Quantidade,
                command.PremioUnitario,
                command.Taxas);

            opcao.AtualizarResultadoInformado(
                command.ResultadoInformado);

            var situacao =
                command.Situacao
                    .Trim()
                    .ToUpperInvariant();

            if (situacao != "ENCERRADA" &&
                situacao != "EXECUTADA")
            {
                throw new ArgumentException(
                    "A situação deve ser ENCERRADA ou EXECUTADA.");
            }

            opcao.AtualizarFinalizacao(
                situacao,
                command.DataFinalizacao,
                command.PrecoRecompraUnitario,
                command.ValorExecucao);

            await _repository.SalvarAlteracoesAsync(
                cancellationToken);
        }
    }
}