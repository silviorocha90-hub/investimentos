using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class DescontoFiscalRepository : IDescontoFiscalRepository
    {
        private readonly InvestimentosDbContext _context;

        public DescontoFiscalRepository(InvestimentosDbContext context) => _context = context;

        public async Task AdicionarAsync(DescontoFiscal desconto, CancellationToken cancellationToken = default)
        {
            await _context.DescontosFiscais.AddAsync(desconto, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<Investidor?> ObterInvestidorAsync(Guid investidorId, CancellationToken cancellationToken = default) =>
            _context.Investidores.FirstOrDefaultAsync(x => x.Id == investidorId, cancellationToken);

        public async Task<IReadOnlyList<DescontoFiscal>> ListarAsync(Guid? investidorId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.DescontosFiscais.AsNoTracking().Include(x => x.Investidor).AsQueryable();
            if (investidorId.HasValue)
                query = query.Where(x => x.InvestidorId == investidorId.Value);

            return await query.OrderByDescending(x => x.DataPagamento).ToListAsync(cancellationToken);
        }

        public Task<decimal> ObterTotalAsync(Guid investidorId, CancellationToken cancellationToken = default) =>
            _context.DescontosFiscais
                .Where(x => x.InvestidorId == investidorId)
                .SumAsync(x => x.Valor, cancellationToken);
    }
}
