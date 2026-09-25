namespace Investimentos.Application.Metas
{
    public record MetaAtivoDto(
        Guid Id,
        Guid InvestidorId,
        string Investidor,
        Guid AtivoId,
        string Ticker,
        string NomeAtivo,
        decimal QuantidadeAtual,
        decimal QuantidadeDesejada,
        decimal QuantidadeFaltante,
        decimal PercentualAtingido);

    public record SalvarMetaAtivoRequest(
        Guid InvestidorId,
        Guid AtivoId,
        decimal QuantidadeDesejada);
}
