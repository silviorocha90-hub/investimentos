namespace Investimentos.Importador.Importacao
{
    public record AtivoImportacao(
        string Ticker,
        string TipoOriginal,
        string? TipoAtivoCodigo,
        decimal? PrecoAtual)
    {
        public bool Classificado =>
            !string.IsNullOrWhiteSpace(TipoAtivoCodigo);
    }
}