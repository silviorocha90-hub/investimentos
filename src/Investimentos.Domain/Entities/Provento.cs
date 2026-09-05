namespace Investimentos.Domain.Entities
{
    public class Provento
    {
        public Guid Id { get; private set; }

        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }

        public Guid AtivoId { get; private set; }
        public Ativo Ativo { get; private set; }

        public string Tipo { get; private set; }

        public DateTime DataCom { get; private set; }
        public DateTime DataPagamento { get; private set; }

        public decimal QuantidadeBase { get; private set; }
        public decimal ValorPorUnidade { get; private set; }
        public decimal ValorTotal { get; private set; }

        private Provento()
        {
            Investidor = null!;
            Ativo = null!;
            Tipo = null!;
        }

        public Provento(
            Investidor investidor,
            Ativo ativo,
            string tipo,
            DateTime dataCom,
            DateTime dataPagamento,
            decimal quantidadeBase,
            decimal valorPorUnidade)
        {
            if (investidor is null)
                throw new ArgumentException(
                    "O investidor é obrigatório.");

            if (ativo is null)
                throw new ArgumentException(
                    "O ativo é obrigatório.");

            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException(
                    "O tipo do provento é obrigatório.");

            var tipoNormalizado =
                tipo.Trim().ToUpperInvariant();

            if (tipoNormalizado != "DIVIDENDO" &&
                tipoNormalizado != "JCP" &&
                tipoNormalizado != "RENDIMENTO")
            {
                throw new ArgumentException(
                    "Tipo de provento inválido.");
            }

            if (quantidadeBase <= 0)
                throw new ArgumentException(
                    "A quantidade base deve ser maior que zero.");

            if (valorPorUnidade < 0)
                throw new ArgumentException(
                    "O valor por unidade não pode ser negativo.");

            if (dataPagamento.Date < dataCom.Date)
                throw new ArgumentException(
                    "A data de pagamento não pode ser anterior à data-com.");

            Id = Guid.NewGuid();
            Investidor = investidor;
            Ativo = ativo;
            Tipo = tipoNormalizado;
            DataCom = dataCom;
            DataPagamento = dataPagamento;
            QuantidadeBase = quantidadeBase;
            ValorPorUnidade = valorPorUnidade;
            ValorTotal = quantidadeBase * valorPorUnidade;
        }
    }
}