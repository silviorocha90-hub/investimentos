namespace Investimentos.Domain.Entities;

public class RelatorioDiarioIa
{
    private RelatorioDiarioIa() { }

    public RelatorioDiarioIa(
        DateTime dataReferencia,
        string conteudo,
        string modelo)
    {
        Id = Guid.NewGuid();
        DataReferencia = dataReferencia.Date;
        DataGeracao = DateTime.UtcNow;
        Conteudo = conteudo;
        Modelo = modelo;
    }

    public Guid Id { get; private set; }
    public DateTime DataReferencia { get; private set; }
    public DateTime DataGeracao { get; private set; }
    public string Conteudo { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
}
