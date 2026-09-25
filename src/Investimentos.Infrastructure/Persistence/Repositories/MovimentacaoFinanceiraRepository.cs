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