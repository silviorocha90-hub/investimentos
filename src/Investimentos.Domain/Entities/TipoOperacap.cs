namespace Investimentos.Domain.Entities
{
    public class TipoOperacao
    {
        public int Id { get; private set; }
        public string Codigo { get; private set; }
        public string Nome { get; private set; }
        public bool Ativo { get; private set; }

        private TipoOperacao()
        {
            Codigo = null!;
            Nome = null!;
        }

        public TipoOperacao(string codigo, string nome)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException(
                    "O código do tipo de operação é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome do tipo de operação é obrigatório.");
            }

            Codigo = codigo.Trim().ToUpperInvariant();
            Nome = nome.Trim();
            Ativo = true;
        }
    }
}