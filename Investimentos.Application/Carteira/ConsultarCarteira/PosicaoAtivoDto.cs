namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public record PosicaoAtivoDto(
        string Ticker,
        string Nome,
        decimal Quantidade,
        decimal PrecoMedio,
        decimal CustoTotal,
        decimal ResultadoRealizado,
        DateTime? DataPrimeiraCompra = null);
}
