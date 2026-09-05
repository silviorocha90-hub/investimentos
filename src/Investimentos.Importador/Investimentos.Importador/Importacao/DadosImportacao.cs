namespace Investimentos.Importador.Importacao
{
    public class DadosImportacao
    {
        public List<OperacaoImportacao> Operacoes { get; } = [];

        public List<ProventoImportacao> Proventos { get; } = [];

        public List<OpcaoImportacao> Opcoes { get; } = [];

        public List<AtivoImportacao> CadastroAtivos { get; } = [];

        public List<CotacaoAtivoImportacao> Cotacoes { get; } = [];

        public List<SaldoDisponivelImportacao> SaldosDisponiveis { get; } = [];

        public List<HistoricoPatrimonioImportacao> HistoricosPatrimonio { get; } = [];

        public List<string> Pendencias { get; } = [];

        public HashSet<string> Ativos { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Investidores { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}