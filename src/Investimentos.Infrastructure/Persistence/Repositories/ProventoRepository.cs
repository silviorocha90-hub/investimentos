using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class ProventoRepository : IProventoRepository
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

        public async Task<Provento?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Proventos
                .Include(x => x.Investidor)
                .Include(x => x.Ativo)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
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
            var tickerNormalizado =
                ticker
                    .Trim()
                    .ToUpperInvariant();

            var ativos =
                await _context.Ativos
                    .ToListAsync(
                        cancellationToken);

            return ativos
                .FirstOrDefault(
                    x =>
                        x.Ticker.Codigo ==
                        tickerNormalizado);
        }

        public async Task<IReadOnlyList<Provento>> ListarAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            var proventos =
                await _context.Proventos
                    .AsNoTracking()
                    .Include(x => x.Ativo)
                    .Where(
                        x =>
                            x.InvestidorId ==
                            investidorId)
                    .OrderByDescending(
                        x =>
                            x.DataPagamento)
                    .ToListAsync(
                        cancellationToken);

            return proventos
                .OrderByDescending(
                    x =>
                        x.DataPagamento)
                .ThenBy(
                    x =>
                        x.Ativo.Ticker.Codigo)
                .ToList();
        }

        public void Excluir(
            Provento provento)
        {
            _context.Proventos.Remove(
                provento);
        }

        public async Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}