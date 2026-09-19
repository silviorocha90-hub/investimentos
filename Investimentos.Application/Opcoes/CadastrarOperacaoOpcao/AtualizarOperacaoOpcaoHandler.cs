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

            if (opcao.Situacao == "ABERTA")
            {
                if (situacao == "ENCERRADA")
                {
                    if (!command.DataFinalizacao.HasValue)
                    {
                        throw new ArgumentException(
                            "A data de finalização é obrigatória para encerrar a opção.");
                    }

                    if (!command.PrecoRecompraUnitario.HasValue)
                    {
                        throw new ArgumentException(
                            "O preço de recompra é obrigatório para encerrar a opção.");
                    }

                    opcao.Encerrar(
                        command.DataFinalizacao.Value,
                        command.PrecoRecompraUnitario.Value);
                }
                else if (situacao == "EXECUTADA")
                {
                    opcao.MarcarExercida(
                        command.ValorExecucao);
                }
                else if (situacao == "EXPIRADA")
                {
                    opcao.MarcarExpirada(
                        command.DataFinalizacao);
                }
                else if (situacao != "ABERTA")
                {
                    throw new ArgumentException(
                        "A situação deve ser ABERTA, ENCERRADA, EXECUTADA ou EXPIRADA.");
                }
            }
            else if (situacao != opcao.Situacao)
            {
                throw new InvalidOperationException(
                    "Uma opção já finalizada não pode ter sua situação alterada.");
            }

            await _repository.SalvarAlteracoesAsync(
                cancellationToken);
        }
    }
}