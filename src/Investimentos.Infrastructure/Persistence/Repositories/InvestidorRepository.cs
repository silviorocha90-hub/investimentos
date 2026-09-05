using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class InvestidorRepository
        : IInvestidorRepository
    {
        private readonly InvestimentosDbContext _context;

        public InvestidorRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(
            Investidor investidor,
            CancellationToken cancellationToken = default)
        {
            await _context.Investidores.AddAsync(
                investidor,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<bool> ExistePorNomeAsync(
            string nome,
            CancellationToken cancellationToken = default)
        {
            return await _context.Investidores
                .AnyAsync(
                    x => x.Nome == nome,
                    cancellationToken);
        }
    }
}