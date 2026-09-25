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

        public decimal? ResultadoBruto =>
            ResultadoInformado ??
            ResultadoFinal;

        public bool EhDayTrade =>
            DataFinalizacao.HasValue &&
            DataFinalizacao.Value.Date ==
            DataOperacao.Date;

        public string RegimeTributario =>
            EhDayTrade
                ? "DAY_TRADE"
                : "COMUM";

        public decimal AliquotaIr =>
            EhDayTrade
                ? 20m
                : 15m;

        public decimal IrEstimado
        {
            get
            {
                var resultado =
                    ResultadoBruto;

                if (!resultado.HasValue ||
                    resultado.Value <= 0)
                {
                    return 0;
                }

                return decimal.Round(
                    resultado.Value *
                    AliquotaIr /
                    100m,
                    2,
                    MidpointRounding.AwayFromZero);
            }
        }

        public decimal? ResultadoLiquido
        {
            get
            {
                var resultado =
                    ResultadoBruto;

                if (!resultado.HasValue)
                {
                    return null;
                }

                return resultado.Value -
                    IrEstimado;
            }
        }

        public bool EstaAtiva =>
            Situacao == "ABERTA" ||
            Situacao == "EXECUTADA";

        public decimal CapitalComprometidoPut =>
            EstaAtiva &&
            TipoOpcao == "PUT" &&
            Natureza == "VENDA"
                ? Strike * Quantidade
                : 0;

        public decimal AcoesComprometidasCall =>
            EstaAtiva &&
            TipoOpcao == "CALL" &&
            Natureza == "VENDA"
                ? Quantidade
                : 0;

        public decimal PremioRecebidoAtivo =>
            EstaAtiva &&
            Natureza == "VENDA"
                ? PremioTotal - Taxas
                : 0;

        public decimal DistanciaStrikePercentual
        {
            get
            {
                if (!ValorAcaoAtual.HasValue ||
                    ValorAcaoAtual.Value <= 0)
                {
                    return 0;
                }

                return
                    (Strike - ValorAcaoAtual.Value) /
                    ValorAcaoAtual.Value *
                    100m;
            }
        }

        public bool EmRiscoExercicio
        {
            get
            {
                if (!EstaAtiva ||
                    !ValorAcaoAtual.HasValue)
                {
                    return false;
                }

                return TipoOpcao switch
                {
                    "PUT" =>
                        ValorAcaoAtual.Value <= Strike,
                    "CALL" =>
                        ValorAcaoAtual.Value >= Strike,
                    _ => false
                };
            }
        }

        public decimal? PercentualGanho
        {
            get
            {
                var resultado =
                    ResultadoLiquido;

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
                    ResultadoLiquido;

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