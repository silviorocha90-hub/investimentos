using Investimentos.Domain.Entities;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorCotacoes
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorCotacoes(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportarAsync(
            IReadOnlyCollection<CotacaoAtivoImportacao> cotacoes,
            IReadOnlyDictionary<string, Ativo> ativos,
            DateTime dataReferencia)
        {
            if (cotacoes is null)
                throw new ArgumentNullException(nameof(cotacoes));

            if (ativos is null)
                throw new ArgumentNullException(nameof(ativos));

            if (dataReferencia == default)
                throw new ArgumentException(
                    "A data de referência das cotações é obrigatória.",
                    nameof(dataReferencia));

            var entidades =
                new List<CotacaoAtivo>();

            var cotacoesOrdenadas =
                cotacoes
                    .OrderBy(x => x.Ativo)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var item in cotacoesOrdenadas)
            {
                if (!ativos.TryGetValue(
                        item.Ativo,
                        out var ativo))
                {
                    throw new InvalidOperationException(
                        $"Ativo '{item.Ativo}' não encontrado para " +
                        $"importação da cotação. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (item.Preco < 0)
                {
                    throw new InvalidOperationException(
                        $"Cotação inválida para o ativo " +
                        $"'{item.Ativo}': {item.Preco}. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var cotacao =
                    new CotacaoAtivo(
                        ativo,
                        dataReferencia,
                        item.Preco);

                entidades.Add(cotacao);
            }

            await _context.CotacoesAtivos
                .AddRangeAsync(entidades);

            await _context.SaveChangesAsync();

            return entidades.Count;
        }
    }
}