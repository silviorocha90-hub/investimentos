namespace Investimentos.Application.Operacoes.AtualizarOperacao
{
    public record AtualizarOperacaoCommand(
        Guid Id,
        DateTime Data,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal Taxas);
}
