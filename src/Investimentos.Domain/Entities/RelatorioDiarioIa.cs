namespace Investimentos.Domain.Entities;

public class RelatorioDiarioIa
{
    private RelatorioDiarioIa() { }

    public RelatorioDiarioIa(
        DateTime dataReferencia,
        string conteudo,
        string modelo,
        string escopo = "TODOS",
        Guid? investidorId = null)
    {
        Id = Guid.NewGuid();
        DataReferencia = dataReferencia.Date;
        DataGeracao = DateTime.UtcNow;
        Conteudo = conteudo;
        Modelo = modelo;
        Escopo = escopo;
        InvestidorId = investidorId;
    }

    public Guid Id { get; private set; }
    public DateTime DataReferencia { get; private set; }
    public DateTime DataGeracao { get; private set; }
    public string Conteudo { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public string Escopo { get; private set; } = "TODOS";
    public Guid? InvestidorId { get; private set; }
}
