namespace Investimentos.Domain.Entities
{
    public class Investidor
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }

        private Investidor()
        {
            Nome = null!;
        }

        public Investidor(string nome)
        {
            ValidarNome(nome);

            Id = Guid.NewGuid();
            Nome = nome.Trim();
        }

        public void AlterarNome(string novoNome)
        {
            ValidarNome(novoNome);

            Nome = novoNome.Trim();
        }

        private static void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome do investidor é obrigatório.");
            }
        }
    }
}