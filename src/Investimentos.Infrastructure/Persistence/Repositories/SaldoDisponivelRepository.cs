using Investimentos.Application.Interfaces;
using Investimentos.Application.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class SaldoDisponivelRepository : ISaldoDisponivelRepository
    {
        private readonly InvestimentosDbContext _context;

        public SaldoDisponivelRepository(InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ObterTotalAtualAsync(
            CancellationToken cancellationToken = default)
        {
            var saldos = await _context.SaldosDisponiveis
                .AsNoTracking()
                .GroupBy(x => x.InvestidorId)
                .Select(grupo => grupo
                    .OrderByDescending(x => x.DataReferencia)
                    .Select(x => x.Valor)
                    .First())
                .ToListAsync(cancellationToken);

            return saldos.Sum();
        }

        public async Task<IReadOnlyList<SaldoInvestidorDto>> ListarAtuaisAsync(
            CancellationToken cancellationToken = default)
        {
            var saldos = await _context.SaldosDisponiveis
                .AsNoTracking()
                .Include(x => x.Investidor)
                .GroupBy(x => x.InvestidorId)
                .Select(grupo => grupo
                    .OrderByDescending(x => x.DataReferencia)
                    .Select(x => new SaldoInvestidorDto(
                        x.Investidor.Nome,
                        x.Valor))
                    .First())
                .ToListAsync(cancellationToken);

            return saldos.OrderByDescending(x => x.Valor).ToList();
        }
    }
}
