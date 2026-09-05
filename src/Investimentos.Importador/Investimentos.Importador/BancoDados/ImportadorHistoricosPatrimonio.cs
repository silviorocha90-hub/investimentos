using Investimentos.Domain.Entities;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorHistoricosPatrimonio
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorHistoricosPatrimonio(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportarAsync(
            IReadOnlyCollection<HistoricoPatrimonioImportacao> historicos,
            IReadOnlyDictionary<string, Investidor> investidores)
        {
            if (historicos is null)
                throw new ArgumentNullException(nameof(historicos));

            if (investidores is null)
                throw new ArgumentNullException(nameof(investidores));

            var entidades =
                new List<HistoricoPatrimonio>();

            var historicosOrdenados =
                historicos
                    .OrderBy(x => x.DataReferencia)
                    .ThenBy(x => x.Investidor)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var item in historicosOrdenados)
            {
                if (!investidores.TryGetValue(
                        item.Investidor,
                        out var investidor))
                {
                    throw new InvalidOperationException(
                        $"Investidor '{item.Investidor}' não encontrado " +
                        $"para importação do histórico patrimonial. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (item.DataReferencia == default)
                {
                    throw new InvalidOperationException(
                        $"Data de referência inválida no histórico " +
                        $"patrimonial de '{item.Investidor}'. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (item.ValorCarteira < 0)
                {
                    throw new InvalidOperationException(
                        $"Valor da carteira inválido para " +
                        $"'{item.Investidor}': {item.ValorCarteira}. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var historico =
                    new HistoricoPatrimonio(
                        investidor,
                        item.DataReferencia,
                        item.ValorCarteira);

                entidades.Add(historico);
            }

            await _context.HistoricosPatrimonio
                .AddRangeAsync(entidades);

            await _context.SaveChangesAsync();

            return entidades.Count;
        }
    }
}