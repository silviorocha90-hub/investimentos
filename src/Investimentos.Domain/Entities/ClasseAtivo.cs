namespace Investimentos.Domain.Entities
{
    public class ClasseAtivo
    {
        public int Id { get; private set; }
        public string Codigo { get; private set; }
        public string Nome { get; private set; }
        public bool Ativo { get; private set; }

        private ClasseAtivo()
        {
            Codigo = null!;
            Nome = null!;
        }

        public ClasseAtivo(
            string codigo,
            string nome)
        {
            Atualizar(
                codigo,
                nome,
                true);
        }

        public void Atualizar(
            string codigo,
            string nome,
            bool ativo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException(
                    "O código da classe do ativo é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome da classe do ativo é obrigatório.");
            }

            Codigo =
                codigo.Trim().ToUpperInvariant();
            Nome = nome.Trim();
            Ativo = ativo;
        }
    }
}
