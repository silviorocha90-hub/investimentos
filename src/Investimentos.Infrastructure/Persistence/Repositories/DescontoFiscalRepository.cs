using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class DescontoFiscalRepository : IDescontoFiscalRepository
    {
        private readonly InvestimentosDbContext _context;

        public DescontoFiscalRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(
            DescontoFiscal desconto,
            CancellationToken cancellationToken = default)
        {
            if (desconto is null)
            {
                throw new ArgumentNullException(
                    nameof(desconto));
            }

            await _context.DescontosFiscais.AddAsync(
                desconto,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<IReadOnlyList<DescontoFiscal>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.DescontosFiscais
                .AsNoTracking()
                .OrderByDescending(
                    x => x.DataPagamento)
                .ThenBy(
                    x => x.Tipo)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<DescontoFiscal?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.DescontosFiscais
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public void Excluir(
            DescontoFiscal desconto)
        {
            _context.DescontosFiscais.Remove(
                desconto);
        }

        public async Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<decimal> ObterTotalAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.DescontosFiscais
                .SumAsync(
                    x => x.Valor,
                    cancellationToken);
        }
    }
}