namespace Investimentos.Application.Proventos.ConsultarProventos
{
    public record ProventoDto(
        Guid Id,
        string Ticker,
        string Tipo,
        DateTime? DataCom,
        DateTime DataPagamento,
        decimal QuantidadeBase,
        decimal ValorPorUnidade,
        decimal ValorBruto,
        decimal ValorRecebido,
        decimal ImpostoRetido);
}