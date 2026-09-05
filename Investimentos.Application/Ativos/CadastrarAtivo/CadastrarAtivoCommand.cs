namespace Investimentos.Application.Ativos.CadastrarAtivo
{
    public record CadastrarAtivoCommand(
        string Ticker,
        string Nome,
        string TipoAtivoCodigo);
}