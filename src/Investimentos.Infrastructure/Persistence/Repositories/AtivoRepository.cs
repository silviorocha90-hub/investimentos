using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class AtivoRepository : IAtivoRepository
    {
        private readonly InvestimentosDbContext _context;

        public AtivoRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(
            Ativo ativo,
            CancellationToken cancellationToken = default)
        {
            await _context.Ativos.AddAsync(
                ativo,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<bool> ExistePorTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ativos
                .AnyAsync(
                    x => x.Ticker == new Domain.ValueObjects.Ticker(ticker),
                    cancellationToken);
        }

        public async Task<TipoAtivo?> ObterTipoAtivoPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default)
        {
            return await _context.TiposAtivos
                .FirstOrDefaultAsync(
                    x => x.Codigo == codigo && x.Ativo,
                    cancellationToken);
        }
    }
}