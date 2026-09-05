using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Entities
{
    public class Ativo
    {
        public Guid Id { get; private set; }
        public Ticker Ticker { get; private set; }
        public string Nome { get; private set; }

        public int TipoAtivoId { get; private set; }
        public TipoAtivo TipoAtivo { get; private set; }

        private Ativo()
        {
            Ticker = null!;
            Nome = null!;
            TipoAtivo = null!;
        }

        public Ativo(
            Ticker ticker,
            string nome,
            TipoAtivo tipoAtivo)
        {
            if (ticker is null)
            {
                throw new ArgumentException(
                    "O ticker do ativo é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome do ativo é obrigatório.");
            }

            if (tipoAtivo is null)
            {
                throw new ArgumentException(
                    "O tipo do ativo é obrigatório.");
            }

            Id = Guid.NewGuid();
            Ticker = ticker;
            Nome = nome.Trim();
            TipoAtivo = tipoAtivo;
        }
    }
}