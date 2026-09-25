namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public record PosicaoAtivoDto(
        string Ticker,
        string Nome,
        string TipoAtivoCodigo,
        string TipoAtivoNome,
        decimal Quantidade,
        decimal PrecoMedio,
        decimal CustoTotal,
        decimal ResultadoRealizado,
        DateTime? DataPrimeiraCompra = null)
    {
        public decimal? PrecoAtual { get; init; }

        public decimal ValorAtual { get; init; }

        public decimal Valorizacao { get; init; }

        public decimal Proventos { get; init; }

        public decimal ResultadoOpcoes { get; init; }

        public decimal ResultadoEconomico { get; init; }

        public decimal RentabilidadeEconomica { get; init; }
    }
}