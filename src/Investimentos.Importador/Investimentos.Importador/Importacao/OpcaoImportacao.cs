namespace Investimentos.Importador.Importacao
{
    public record OpcaoImportacao(
        int LinhaExcel,
        DateTime DataOperacao,
        DateTime? DataFinalizacao,
        string Investidor,
        string TickerOpcao,
        string TipoOpcao,
        string Natureza,
        decimal Quantidade,
        decimal Strike,
        DateTime Vencimento,
        decimal PremioUnitario,
        decimal? PrecoRecompraUnitario,
        decimal? ResultadoInformado,
        decimal? ValorExecucao,
        string Situacao);
}