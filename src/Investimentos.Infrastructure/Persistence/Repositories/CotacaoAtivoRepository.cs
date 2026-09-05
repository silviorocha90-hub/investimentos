using Investimentos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class CotacaoAtivoRepository : ICotacaoAtivoRepository
    {
        private readonly InvestimentosDbContext _context;

        public CotacaoAtivoRepository(InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyDictionary<string, decimal>>
            ObterUltimasPorTickerAsync(
                CancellationToken cancellationToken = default)
        {
            var cotacoes = await _context.CotacoesAtivos
                .AsNoTracking()
                .Include(x => x.Ativo)
                .GroupBy(x => x.AtivoId)
                .Select(grupo => grupo
                    .OrderByDescending(x => x.DataReferencia)
                    .Select(x => new
                    {
                        Ticker = x.Ativo.Ticker.Codigo,
                        x.Preco
                    })
                    .First())
                .ToListAsync(cancellationToken);

            return cotacoes.ToDictionary(
                x => x.Ticker,
                x => x.Preco);
        }
    }
}
