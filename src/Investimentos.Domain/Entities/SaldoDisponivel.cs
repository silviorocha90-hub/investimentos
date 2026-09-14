namespace Investimentos.Domain.Entities
{
    public class SaldoDisponivel
    {
        public Guid Id { get; private set; }

        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }

        public DateTime DataReferencia { get; private set; }

        public decimal Valor { get; private set; }

        private SaldoDisponivel()
        {
            Investidor = null!;
        }

        public SaldoDisponivel(
            Investidor investidor,
            DateTime dataReferencia,
            decimal valor)
        {
            if (investidor is null)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            Id = Guid.NewGuid();

            Investidor = investidor;
            DataReferencia = dataReferencia;
            Valor = valor;
        }

        public void Atualizar(
            DateTime dataReferencia,
            decimal valor)
        {
            DataReferencia = dataReferencia;
            Valor = valor;
        }
    }
}