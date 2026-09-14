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
            ValidarTicker(ticker);
            ValidarNome(nome);
            ValidarTipoAtivo(tipoAtivo);

            Id = Guid.NewGuid();
            Ticker = ticker;
            Nome = nome.Trim();
            TipoAtivo = tipoAtivo;
        }

        public void Atualizar(
            string nome,
            TipoAtivo tipoAtivo)
        {
            ValidarNome(nome);
            ValidarTipoAtivo(tipoAtivo);

            Nome = nome.Trim();
            TipoAtivo = tipoAtivo;
            TipoAtivoId = tipoAtivo.Id;
        }

        private static void ValidarTicker(
            Ticker ticker)
        {
            if (ticker is null)
            {
                throw new ArgumentException(
                    "O ticker do ativo é obrigatório.");
            }
        }

        private static void ValidarNome(
            string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome do ativo é obrigatório.");
            }
        }

        private static void ValidarTipoAtivo(
            TipoAtivo tipoAtivo)
        {
            if (tipoAtivo is null)
            {
                throw new ArgumentException(
                    "O tipo do ativo é obrigatório.");
            }
        }
    }
}