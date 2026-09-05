namespace Investimentos.Domain.Entities
{
    public class CotacaoAtivo
    {
        public Guid Id { get; private set; }

        public Guid AtivoId { get; private set; }
        public Ativo Ativo { get; private set; }

        public DateTime DataReferencia { get; private set; }

        public decimal Preco { get; private set; }

        private CotacaoAtivo()
        {
            Ativo = null!;
        }

        public CotacaoAtivo(
            Ativo ativo,
            DateTime dataReferencia,
            decimal preco)
        {
            if (ativo is null)
            {
                throw new ArgumentException(
                    "O ativo é obrigatório.");
            }

            if (preco < 0)
            {
                throw new ArgumentException(
                    "O preço da cotação não pode ser negativo.");
            }

            Id = Guid.NewGuid();

            Ativo = ativo;
            DataReferencia = dataReferencia;
            Preco = preco;
        }
    }
}