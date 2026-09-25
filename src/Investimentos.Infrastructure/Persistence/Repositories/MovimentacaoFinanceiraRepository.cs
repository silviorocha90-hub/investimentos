using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class MovimentacaoFinanceiraRepository : IMovimentacaoFinanceiraRepository
    {
        private readonly InvestimentosDbContext _context;

        public MovimentacaoFinanceiraRepository(InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<MovimentacaoFinanceira>> ListarAsync(
            Guid? investidorId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.MovimentacoesFinanceiras
                .AsNoTracking()
                .AsQueryable();

            if (investidorId.HasValue)
            {
                query = query.Where(x => x.InvestidorId == investidorId.Value);
            }

            return await query
                .OrderBy(x => x.Data)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<MovimentacaoFinanceira>> ListarAnoAsync(
            Guid investidorId,
            int ano,
            CancellationToken cancellationToken = default)
        {
            var inicio = new DateTime(ano, 1, 1);
            var fim = inicio.AddYears(1);

            return await _context.MovimentacoesFinanceiras
                .AsNoTracking()
                .Where(x =>
                    x.InvestidorId == investidorId &&
                    x.Data >= inicio &&
                    x.Data < fim)
                .ToListAsync(cancellationToken);
        }
    }
}