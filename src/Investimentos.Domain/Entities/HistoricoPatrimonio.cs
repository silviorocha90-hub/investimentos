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
            ValidarInvestidor(investidor);
            ValidarValor(valorCarteira);

            Id = Guid.NewGuid();

            Investidor = investidor;
            DataReferencia = dataReferencia;
            ValorCarteira = valorCarteira;
        }

        public void Atualizar(
            DateTime dataReferencia,
            decimal valorCarteira)
        {
            ValidarValor(valorCarteira);

            DataReferencia = dataReferencia;
            ValorCarteira = valorCarteira;
        }

        private static void ValidarInvestidor(
            Investidor investidor)
        {
            if (investidor is null)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }
        }

        private static void ValidarValor(
            decimal valorCarteira)
        {
            if (valorCarteira < 0)
            {
                throw new ArgumentException(
                    "O valor da carteira não pode ser negativo.");
            }
        }
    }
}