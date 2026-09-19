namespace Investimentos.Domain.Entities
{
    public class TipoAtivo
    {
        public int Id { get; private set; }
        public string Codigo { get; private set; }
        public string Nome { get; private set; }
        public bool Ativo { get; private set; }

        public int ClasseAtivoId { get; private set; }
        public ClasseAtivo ClasseAtivo { get; private set; }

        private TipoAtivo()
        {
            Codigo = null!;
            Nome = null!;
            ClasseAtivo = null!;
        }

        public TipoAtivo(
            string codigo,
            string nome,
            ClasseAtivo classeAtivo)
        {
            if (classeAtivo is null)
            {
                throw new ArgumentException(
                    "A classe do ativo é obrigatória.");
            }

            ClasseAtivo = classeAtivo;
            ClasseAtivoId = classeAtivo.Id;

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
                    "O código do tipo do ativo é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome do tipo do ativo é obrigatório.");
            }

            Codigo =
                codigo.Trim().ToUpperInvariant();
            Nome = nome.Trim();
            Ativo = ativo;
        }
    }
}
