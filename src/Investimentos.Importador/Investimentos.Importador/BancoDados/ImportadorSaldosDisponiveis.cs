using Investimentos.Domain.Entities;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorSaldosDisponiveis
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorSaldosDisponiveis(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportarAsync(
            IReadOnlyCollection<SaldoDisponivelImportacao> saldos,
            IReadOnlyDictionary<string, Investidor> investidores,
            DateTime dataReferencia)
        {
            if (saldos is null)
                throw new ArgumentNullException(nameof(saldos));

            if (investidores is null)
                throw new ArgumentNullException(nameof(investidores));

            if (dataReferencia == default)
                throw new ArgumentException(
                    "A data de referência dos saldos é obrigatória.",
                    nameof(dataReferencia));

            var entidades =
                new List<SaldoDisponivel>();

            var saldosOrdenados =
                saldos
                    .OrderBy(x => x.Investidor)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var item in saldosOrdenados)
            {
                if (!investidores.TryGetValue(
                        item.Investidor,
                        out var investidor))
                {
                    throw new InvalidOperationException(
                        $"Investidor '{item.Investidor}' não encontrado " +
                        $"para importação do saldo disponível. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var saldo =
                    new SaldoDisponivel(
                        investidor,
                        dataReferencia,
                        item.Valor);

                entidades.Add(saldo);
            }

            await _context.SaldosDisponiveis
                .AddRangeAsync(entidades);

            await _context.SaveChangesAsync();

            return entidades.Count;
        }
    }
}