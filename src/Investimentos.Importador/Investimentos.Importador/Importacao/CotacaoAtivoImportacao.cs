namespace Investimentos.Importador.Importacao
{
    public record CotacaoAtivoImportacao(
        int LinhaExcel,
        string Ativo,
        decimal Preco);
}