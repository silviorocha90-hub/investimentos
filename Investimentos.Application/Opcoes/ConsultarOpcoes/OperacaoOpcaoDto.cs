namespace Investimentos.Application.Opcoes.ConsultarOpcoes
{
    public record OperacaoOpcaoDto(
        Guid Id,
        string TickerAtivo,
        string TickerOpcao,
        string TipoOpcao,
        string Natureza,
        DateTime DataOperacao,
        DateTime Vencimento,
        DateTime? DataFinalizacao,
        decimal Strike,
        int Contratos,
        decimal Quantidade,
        decimal PremioUnitario,
        decimal PremioTotal,
        decimal Taxas,
        decimal? PrecoRecompraUnitario,
        decimal? ValorRecompraTotal,
        decimal? ValorExecucao,
        decimal? ResultadoInformado,
        decimal? ResultadoFinal,
        string Situacao);
}
