namespace Investimentos.Domain.Entities
{
    public class MovimentacaoFinanceira
    {
        public Guid Id { get; private set; }
        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; } = null!;
        public DateTime Data { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public string? Descricao { get; private set; }

        private MovimentacaoFinanceira() { }

        public MovimentacaoFinanceira(
            Investidor investidor,
            DateTime data,
            string tipo,
            decimal valor,
            string? descricao = null)
        {
            if (investidor is null)
                throw new ArgumentException("O investidor é obrigatório.");
            if (tipo != "APORTE" && tipo != "RETIRADA")
                throw new ArgumentException("O tipo deve ser APORTE ou RETIRADA.");
            if (valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.");

            Id = Guid.NewGuid();
            Investidor = investidor;
            InvestidorId = investidor.Id;
            Data = data.Date;
            Tipo = tipo;
            Valor = valor;
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        }
    }
}