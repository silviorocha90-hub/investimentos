namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public record OperacaoCarteiraDto(
        Guid Id,
        Guid AtivoId,
        string Ticker,
        string Nome,
        string TipoOperacao,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal Taxas,
        DateTime Data,
        int Sequencia);
}