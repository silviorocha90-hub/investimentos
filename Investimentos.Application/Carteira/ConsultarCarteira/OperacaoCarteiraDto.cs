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
        int Sequencia)
    {
        public string TipoAtivoCodigo { get; init; } = string.Empty;

        public string TipoAtivoNome { get; init; } = string.Empty;
    }
}