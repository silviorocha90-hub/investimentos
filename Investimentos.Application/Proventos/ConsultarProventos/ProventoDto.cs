namespace Investimentos.Application.Proventos.ConsultarProventos
{
    public record ProventoDto(
        Guid Id,
        string Ticker,
        string Tipo,
        string? Descricao,
        DateTime? DataCom,
        DateTime DataPagamento,
        decimal QuantidadeBase,
        decimal ValorPorUnidade,
        decimal ValorBruto,
        decimal ValorRecebido,
        decimal ImpostoRetido)
    {
        public decimal ValorLiquido =>
            ValorRecebido;

        public decimal IrEfetivo =>
            ImpostoRetido;

        public decimal AliquotaEfetiva
        {
            get
            {
                if (ValorBruto <= 0)
                {
                    return 0;
                }

                return
                    ImpostoRetido /
                    ValorBruto *
                    100m;
            }
        }
    }
}