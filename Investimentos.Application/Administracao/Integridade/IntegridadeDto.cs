namespace Investimentos.Application.Administracao.Integridade
{
    public record IntegridadeDto(
        bool Integro,
        int TotalProblemas,
        IReadOnlyList<ProblemaIntegridadeDto> Problemas);

    public record ProblemaIntegridadeDto(
        string Codigo,
        string Severidade,
        string Entidade,
        string Referencia,
        string Mensagem);
}
