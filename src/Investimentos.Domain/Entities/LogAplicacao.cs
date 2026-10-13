namespace Investimentos.Domain.Entities;

public sealed class LogAplicacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DataUtc { get; set; } = DateTime.UtcNow;
    public string Ambiente { get; set; } = "";
    public string Modulo { get; set; } = "";
    public string Operacao { get; set; } = "";
    public string Nivel { get; set; } = "ERRO";
    public string Mensagem { get; set; } = "";
    public string? Detalhes { get; set; }
    public string? CorrelacaoId { get; set; }
}
