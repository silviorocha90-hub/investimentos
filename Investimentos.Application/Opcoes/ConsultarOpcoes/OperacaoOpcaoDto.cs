namespace Investimentos.Application.Opcoes.ConsultarOpcoes
{
    public record OperacaoOpcaoDto(
        Guid Id,
        string TickerAtivo,
        string TickerOpcao,
        string TipoOpcao,
        string Natureza,
        DateTime DataOperacao,
        DateTime Vencimento,
        DateTime? DataFinalizacao,
        decimal Strike,
        int Contratos,
        decimal Quantidade,
        decimal PremioUnitario,
        decimal PremioTotal,
        decimal Taxas,
        decimal? PrecoRecompraUnitario,
        decimal? ValorRecompraTotal,
        decimal? ValorExecucao,
        decimal? ResultadoInformado,
        decimal? ResultadoFinal,
        string Situacao)
    {
        public decimal? ValorAcaoAtual { get; init; }

        public decimal? PercentualGanho
        {
            get
            {
                var resultado =
                    ResultadoInformado ??
                    ResultadoFinal;

                var capital =
                    Strike * Quantidade;

                if (!resultado.HasValue ||
                    capital == 0)
                {
                    return null;
                }

                return resultado.Value /
                    capital *
                    100m;
            }
        }

        public decimal? PercentualGanhoPremio
        {
            get
            {
                var resultado =
                    ResultadoInformado ??
                    ResultadoFinal;

                if (!resultado.HasValue ||
                    PremioTotal == 0)
                {
                    return null;
                }

                return resultado.Value /
                    PremioTotal *
                    100m;
            }
        }
    }
}