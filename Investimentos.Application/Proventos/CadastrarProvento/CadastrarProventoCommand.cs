namespace Investimentos.Application.Proventos.CadastrarProvento
{
    public record CadastrarProventoCommand(
        Guid InvestidorId,
        string Ticker,
        string Tipo,
        DateTime? DataCom,
        DateTime DataPagamento,
        decimal QuantidadeBase,
        decimal ValorPorUnidade,
        decimal ValorRecebido,
        string? Descricao);
}