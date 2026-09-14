using Investimentos.Application.Administracao;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class InvestidorAdministracaoRepository :
        IInvestidorAdministracaoRepository
    {
        private readonly InvestimentosDbContext _context;

        public InvestidorAdministracaoRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public Task<Investidor?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _context.Investidores
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}