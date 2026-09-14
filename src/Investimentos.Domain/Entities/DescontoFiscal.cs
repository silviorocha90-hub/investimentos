namespace Investimentos.Domain.Entities
{
    public class DescontoFiscal
    {
        public Guid Id { get; private set; }

        public string Tipo { get; private set; }
        public DateTime DataPagamento { get; private set; }
        public decimal Valor { get; private set; }
        public string? Descricao { get; private set; }

        private DescontoFiscal()
        {
            Tipo = null!;
        }

        public DescontoFiscal(
            string tipo,
            DateTime dataPagamento,
            decimal valor,
            string? descricao = null)
        {
            ValidarTipo(tipo);
            ValidarValor(valor);

            Id = Guid.NewGuid();
            Tipo = tipo;
            DataPagamento = dataPagamento;
            Valor = valor;
            Descricao = NormalizarDescricao(descricao);
        }

        public void Atualizar(
            string tipo,
            DateTime dataPagamento,
            decimal valor,
            string? descricao = null)
        {
            ValidarTipo(tipo);
            ValidarValor(valor);

            Tipo = tipo;
            DataPagamento = dataPagamento;
            Valor = valor;
            Descricao = NormalizarDescricao(descricao);
        }

        private static void ValidarTipo(
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException(
                    "O tipo do desconto fiscal é obrigatório.");

            if (tipo != "DARF" &&
                tipo != "SPRAD" &&
                tipo != "DARF_SPRAD")
            {
                throw new ArgumentException(
                    "O tipo deve ser DARF, SPRAD ou DARF_SPRAD.");
            }
        }

        private static void ValidarValor(
            decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException(
                    "O valor deve ser maior que zero.");
        }

        private static string? NormalizarDescricao(
            string? descricao)
        {
            return string.IsNullOrWhiteSpace(descricao)
                ? null
                : descricao.Trim();
        }
    }
}