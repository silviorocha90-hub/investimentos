namespace Investimentos.Importador.Importacao
{
    public record ProventoImportacao(
        int LinhaExcel,
        DateTime DataPagamento,
        string Ativo,
        string Descricao,
        string Investidor,
        decimal QuantidadeBase,
        decimal ValorPorUnidade,
        decimal ValorRecebido,
        string Tipo);
}