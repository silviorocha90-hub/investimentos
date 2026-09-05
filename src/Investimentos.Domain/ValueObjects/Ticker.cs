namespace Investimentos.Domain.ValueObjects
{
    public sealed record Ticker
    {
        public string Codigo { get; }

        public Ticker(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException(
                    "O código do ticker é obrigatório.");
            }

            Codigo = codigo.Trim().ToUpperInvariant();
        }

        public override string ToString()
        {
            return Codigo;
        }
    }
}