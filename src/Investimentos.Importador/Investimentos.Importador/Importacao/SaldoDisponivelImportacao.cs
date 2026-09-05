namespace Investimentos.Importador.Importacao
{
    public record SaldoDisponivelImportacao(
        int LinhaExcel,
        string Investidor,
        decimal Valor);
}