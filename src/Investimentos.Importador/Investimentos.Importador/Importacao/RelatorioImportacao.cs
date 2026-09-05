namespace Investimentos.Importador.Importacao
{
    public class RelatorioImportacao
    {
        public int OperacoesTotal { get; set; }
        public int Compras { get; set; }
        public int Vendas { get; set; }
        public int OperacoesPendentes { get; set; }

        public int ProventosTotal { get; set; }

        public int OpcoesTotal { get; set; }
        public int Puts { get; set; }
        public int Calls { get; set; }

        public HashSet<string> Ativos { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Investidores { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public List<string> Pendencias { get; } = [];
    }
}