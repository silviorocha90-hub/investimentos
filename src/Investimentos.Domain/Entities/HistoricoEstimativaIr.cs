namespace Investimentos.Domain.Entities;

public class HistoricoEstimativaIr
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Ano { get; set; }
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public string CriadoPor { get; set; } = "";
    public string Conteudo { get; set; } = "";
}

public class RetencaoDefinitivaIr
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperacaoId { get; set; }
    public decimal Valor { get; set; }
    public string Observacao { get; set; } = "";
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public string CriadoPor { get; set; } = "";
}
