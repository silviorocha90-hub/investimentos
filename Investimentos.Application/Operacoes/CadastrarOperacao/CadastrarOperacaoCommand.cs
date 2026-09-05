namespace Investimentos.Application.Operacoes.CadastrarOperacao
{
    public record CadastrarOperacaoCommand(
        DateTime Data,
        Guid InvestidorId,
        string Ticker,
        string TipoOperacaoCodigo,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal Taxas);
}