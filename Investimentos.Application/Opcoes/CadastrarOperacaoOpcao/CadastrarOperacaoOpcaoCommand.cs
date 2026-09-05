namespace Investimentos.Application.Opcoes.CadastrarOperacaoOpcao
{
    public record CadastrarOperacaoOpcaoCommand(
        Guid InvestidorId,
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
        decimal Taxas);
}