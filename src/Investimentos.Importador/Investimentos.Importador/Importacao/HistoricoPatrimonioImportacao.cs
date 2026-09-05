namespace Investimentos.Importador.Importacao
{
    public record HistoricoPatrimonioImportacao(
        int LinhaExcel,
        string Investidor,
        DateTime DataReferencia,
        decimal ValorCarteira);
}