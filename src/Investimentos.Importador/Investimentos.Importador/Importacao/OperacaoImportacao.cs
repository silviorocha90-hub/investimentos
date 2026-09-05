namespace Investimentos.Importador.Importacao
{
    public record OperacaoImportacao(
        int LinhaExcel,
        DateTime Data,
        string Ativo,
        string Investidor,
        string TipoOperacao,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal Taxas,
        decimal? CustoTotalInformado,
        decimal? ValorRecebidoInformado);
}