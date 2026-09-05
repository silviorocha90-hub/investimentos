namespace Investimentos.Domain.Entities
{
    public class HistoricoPatrimonio
    {
        public Guid Id { get; private set; }

        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }

        public DateTime DataReferencia { get; private set; }

        public decimal ValorCarteira { get; private set; }

        private HistoricoPatrimonio()
        {
            Investidor = null!;
        }

        public HistoricoPatrimonio(
            Investidor investidor,
            DateTime dataReferencia,
            decimal valorCarteira)
        {
            if (investidor is null)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            if (valorCarteira < 0)
            {
                throw new ArgumentException(
                    "O valor da carteira não pode ser negativo.");
            }

            Id = Guid.NewGuid();

            Investidor = investidor;
            DataReferencia = dataReferencia;
            ValorCarteira = valorCarteira;
        }
    }
}