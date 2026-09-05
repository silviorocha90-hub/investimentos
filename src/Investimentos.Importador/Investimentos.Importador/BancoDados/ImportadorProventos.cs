using Investimentos.Domain.Entities;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorProventos
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorProventos(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportarAsync(
            IReadOnlyCollection<ProventoImportacao> proventos,
            IReadOnlyDictionary<string, Investidor> investidores,
            IReadOnlyDictionary<string, Ativo> ativos)
        {
            if (proventos is null)
            {
                throw new ArgumentNullException(
                    nameof(proventos));
            }

            if (investidores is null)
            {
                throw new ArgumentNullException(
                    nameof(investidores));
            }

            if (ativos is null)
            {
                throw new ArgumentNullException(
                    nameof(ativos));
            }

            var entidades =
                new List<Provento>();

            var proventosOrdenados =
                proventos
                    .OrderBy(x => x.DataPagamento)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var item in proventosOrdenados)
            {
                if (!investidores.TryGetValue(
                        item.Investidor,
                        out var investidor))
                {
                    throw new InvalidOperationException(
                        $"Investidor '{item.Investidor}' " +
                        $"não encontrado. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (!ativos.TryGetValue(
                        item.Ativo,
                        out var ativo))
                {
                    throw new InvalidOperationException(
                        $"Ativo '{item.Ativo}' " +
                        $"não encontrado. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                DateTime? dataCom =
                    null;

                var provento =
                    new Provento(
                        investidor,
                        ativo,
                        item.Tipo,
                        item.Descricao,
                        dataCom,
                        item.DataPagamento,
                        item.QuantidadeBase,
                        item.ValorPorUnidade,
                        item.ValorRecebido);

                entidades.Add(
                    provento);
            }

            await _context.Proventos
                .AddRangeAsync(
                    entidades);

            await _context.SaveChangesAsync();

            return entidades.Count;
        }
    }
}