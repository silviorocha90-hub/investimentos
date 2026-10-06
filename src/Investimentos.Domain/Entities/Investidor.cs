namespace Investimentos.Domain.Entities
{
    public class Investidor
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string? WhatsApp { get; private set; }
        public bool ReceberRelatorioIa { get; private set; }
        public string FrequenciaRelatorioIa { get; private set; } = "DIARIO";

        private Investidor()
        {
            Nome = null!;
        }

        public Investidor(string nome)
        {
            ValidarNome(nome);

            Id = Guid.NewGuid();
            Nome = nome.Trim();
            ReceberRelatorioIa = false;
            FrequenciaRelatorioIa = "DIARIO";
        }

        public void AlterarNome(string novoNome)
        {
            ValidarNome(novoNome);

            Nome = novoNome.Trim();
        }

        public void ConfigurarRelatorioIa(
            string? whatsApp,
            bool receberRelatorioIa,
            string? frequencia)
        {
            var numero = string.IsNullOrWhiteSpace(whatsApp)
                ? null
                : new string(whatsApp.Where(char.IsDigit).ToArray());

            if (receberRelatorioIa && string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("Informe o WhatsApp para habilitar o relatório da IA.");

            if (numero is not null && (numero.Length < 10 || numero.Length > 15))
                throw new ArgumentException("O WhatsApp informado é inválido.");

            var frequenciaNormalizada = (frequencia ?? "DIARIO").Trim().ToUpperInvariant();
            var frequenciasValidas = new[] { "DIARIO", "SEMANAL", "QUINZENAL", "MENSAL" };

            if (!frequenciasValidas.Contains(frequenciaNormalizada))
                throw new ArgumentException("Frequência do relatório da IA inválida.");

            WhatsApp = numero;
            ReceberRelatorioIa = receberRelatorioIa;
            FrequenciaRelatorioIa = frequenciaNormalizada;
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