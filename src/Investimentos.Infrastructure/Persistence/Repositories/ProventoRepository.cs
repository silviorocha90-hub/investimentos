using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class ProventoRepository
        : IProventoRepository
    {
        private readonly InvestimentosDbContext _context;

        public ProventoRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(
            Provento provento,
            CancellationToken cancellationToken = default)
        {
            await _context.Proventos.AddAsync(
                provento,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<Investidor?> ObterInvestidorAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Investidores
                .FirstOrDefaultAsync(
                    x => x.Id == investidorId,
                    cancellationToken);
        }

        public async Task<Ativo?> ObterAtivoPorTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default)
        {
            var tickerValueObject =
                new Ticker(ticker);

            return await _context.Ativos
                .FirstOrDefaultAsync(
                    x => x.Ticker == tickerValueObject,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Provento>>
            ListarAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            return await _context.Proventos
                .AsNoTracking()
                .Include(x => x.Ativo)
                .Where(x =>
                    x.InvestidorId == investidorId)
                .OrderByDescending(x =>
                    x.DataPagamento)
                .ToListAsync(
                    cancellationToken);
        }
    }
}