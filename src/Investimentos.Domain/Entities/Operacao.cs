namespace Investimentos.Domain.Entities
{
    public class Operacao
    {
        public Guid Id { get; private set; }
        public DateTime Data { get; private set; }
        public int Sequencia { get; private set; }

        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }

        public Guid AtivoId { get; private set; }
        public Ativo Ativo { get; private set; }

        public int TipoOperacaoId { get; private set; }
        public TipoOperacao TipoOperacao { get; private set; }

        public decimal Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; }
        public decimal Taxas { get; private set; }

        private Operacao()
        {
            Investidor = null!;
            Ativo = null!;
            TipoOperacao = null!;
        }

        public Operacao(
            DateTime data,
            int sequencia,
            Investidor investidor,
            Ativo ativo,
            TipoOperacao tipoOperacao,
            decimal quantidade,
            decimal precoUnitario,
            decimal taxas = 0)
        {
            if (sequencia <= 0)
            {
                throw new ArgumentException(
                    "A sequência deve ser maior que zero.");
            }

            if (investidor is null)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            if (ativo is null)
            {
                throw new ArgumentException(
                    "O ativo é obrigatório.");
            }

            if (tipoOperacao is null)
            {
                throw new ArgumentException(
                    "O tipo de operação é obrigatório.");
            }

            if (quantidade <= 0)
            {
                throw new ArgumentException(
                    "A quantidade deve ser maior que zero.");
            }

            if (precoUnitario < 0)
            {
                throw new ArgumentException(
                    "O preço unitário não pode ser negativo.");
            }

            if (taxas < 0)
            {
                throw new ArgumentException(
                    "As taxas não podem ser negativas.");
            }

            Id = Guid.NewGuid();
            Data = data;
            Sequencia = sequencia;
            Investidor = investidor;
            Ativo = ativo;
            TipoOperacao = tipoOperacao;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
            Taxas = taxas;
        }

        public decimal ValorBruto =>
            Quantidade * PrecoUnitario;
    }
}