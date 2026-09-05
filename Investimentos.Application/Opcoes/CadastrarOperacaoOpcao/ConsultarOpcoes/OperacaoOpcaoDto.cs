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
        decimal Strike,
        int Contratos,
        decimal Quantidade,
        decimal PremioUnitario,
        decimal PremioTotal,
        decimal Taxas,
        string Situacao);
}