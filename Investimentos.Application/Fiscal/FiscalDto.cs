namespace Investimentos.Application.Fiscal
{
    public record FiscalMesDto(
        int Ano,
        int Mes,
        Guid? InvestidorId,
        string Investidor,
        decimal ResultadoComumOpcoes,
        decimal ResultadoDayTradeOpcoes,
        decimal IrEstimadoOpcoes,
        decimal DarfPago,
        decimal DiferencaEstimadoPago,
        int OperacoesConsideradas,
        int Pendencias);

    public record FiscalResumoDto(
        decimal ResultadoComumOpcoes,
        decimal ResultadoDayTradeOpcoes,
        decimal IrEstimadoOpcoes,
        decimal DarfPago,
        decimal DiferencaEstimadoPago,
        int OperacoesConsideradas,
        int Pendencias,
        IReadOnlyList<FiscalMesDto> Meses,
        IReadOnlyList<string> Avisos);
}
