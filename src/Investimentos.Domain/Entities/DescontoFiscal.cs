namespace Investimentos.Domain.Entities
{
    public class DescontoFiscal
    {
        public Guid Id { get; private set; }

        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }

        public string Tipo { get; private set; }
        public DateTime DataPagamento { get; private set; }
        public decimal Valor { get; private set; }
        public string? Descricao { get; private set; }

        private DescontoFiscal()
        {
            Investidor = null!;
            Tipo = null!;
        }

        public DescontoFiscal(
            Investidor investidor,
            string tipo,
            DateTime dataPagamento,
            decimal valor,
            string? descricao = null)
        {
            if (investidor is null)
                throw new ArgumentException("O investidor é obrigatório.");

            if (tipo != "DARF" && tipo != "SPRAD" && tipo != "DARF_SPRAD")
                throw new ArgumentException("O tipo deve ser DARF, SPRAD ou DARF_SPRAD.");

            if (valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.");

            Id = Guid.NewGuid();
            Investidor = investidor;
            Tipo = tipo;
            DataPagamento = dataPagamento;
            Valor = valor;
            Descricao = descricao;
        }
    }
}
