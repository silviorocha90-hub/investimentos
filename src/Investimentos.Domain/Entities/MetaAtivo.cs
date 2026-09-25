namespace Investimentos.Domain.Entities
{
    public class MetaAtivo
    {
        public Guid Id { get; private set; }
        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }
        public Guid AtivoId { get; private set; }
        public Ativo Ativo { get; private set; }
        public decimal QuantidadeDesejada { get; private set; }

        private MetaAtivo()
        {
            Investidor = null!;
            Ativo = null!;
        }

        public MetaAtivo(
            Investidor investidor,
            Ativo ativo,
            decimal quantidadeDesejada)
        {
            if (investidor is null)
                throw new ArgumentException("O investidor é obrigatório.");
            if (ativo is null)
                throw new ArgumentException("O ativo é obrigatório.");
            ValidarQuantidade(quantidadeDesejada);

            Id = Guid.NewGuid();
            Investidor = investidor;
            InvestidorId = investidor.Id;
            Ativo = ativo;
            AtivoId = ativo.Id;
            QuantidadeDesejada = quantidadeDesejada;
        }

        public void Atualizar(decimal quantidadeDesejada)
        {
            ValidarQuantidade(quantidadeDesejada);
            QuantidadeDesejada = quantidadeDesejada;
        }

        private static void ValidarQuantidade(decimal quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade desejada deve ser maior que zero.");
        }
    }
}
