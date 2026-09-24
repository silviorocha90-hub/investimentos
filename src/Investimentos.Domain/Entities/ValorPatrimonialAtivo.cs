namespace Investimentos.Domain.Entities
{
    public class ValorPatrimonialAtivo
    {
        public Guid Id { get; private set; }
        public Guid AtivoId { get; private set; }
        public Ativo Ativo { get; private set; } = null!;
        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; } = null!;
        public DateTime DataReferencia { get; private set; }
        public decimal Valor { get; private set; }

        private ValorPatrimonialAtivo() { }

        public ValorPatrimonialAtivo(
            Ativo ativo,
            Investidor investidor,
            DateTime dataReferencia,
            decimal valor)
        {
            if (ativo is null)
                throw new ArgumentException("O ativo é obrigatório.");
            if (investidor is null)
                throw new ArgumentException("O investidor é obrigatório.");
            if (valor < 0)
                throw new ArgumentException("O valor atual não pode ser negativo.");

            Id = Guid.NewGuid();
            Ativo = ativo;
            AtivoId = ativo.Id;
            Investidor = investidor;
            InvestidorId = investidor.Id;
            DataReferencia = dataReferencia.Date;
            Valor = valor;
        }

        public void Atualizar(DateTime dataReferencia, decimal valor)
        {
            if (valor < 0)
                throw new ArgumentException("O valor atual não pode ser negativo.");
            DataReferencia = dataReferencia.Date;
            Valor = valor;
        }
    }
}
