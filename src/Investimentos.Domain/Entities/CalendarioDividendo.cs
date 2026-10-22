namespace Investimentos.Domain.Entities;

public sealed class CalendarioDividendo
{
    public Guid InvestidorId { get; set; }
    public string Ticker { get; set; } = "";
    public string Meses { get; set; } = "[]";
    public DateTime AtualizadoEmUtc { get; set; }
}
