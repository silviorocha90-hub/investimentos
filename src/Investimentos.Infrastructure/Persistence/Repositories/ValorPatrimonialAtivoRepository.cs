using Investimentos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class ValorPatrimonialAtivoRepository
        : IValorPatrimonialAtivoRepository
    {
        private readonly InvestimentosDbContext _context;

        public ValorPatrimonialAtivoRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyDictionary<string, decimal>>
            ObterUltimosPorTickerAsync(
                CancellationToken cancellationToken = default)
        {
            var valores =
                await _context.ValoresPatrimoniaisAtivos
                    .AsNoTracking()
                    .Include(x => x.Ativo)
                    .OrderByDescending(x => x.DataReferencia)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync(cancellationToken);

            return valores
                .GroupBy(
                    x => x.Ativo.Ticker.Codigo,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x.First().Valor,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
