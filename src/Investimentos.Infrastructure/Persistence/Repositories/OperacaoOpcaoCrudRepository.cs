using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class OperacaoOpcaoCrudRepository
        : IOperacaoOpcaoCrudRepository
    {
        private readonly InvestimentosDbContext _context;

        public OperacaoOpcaoCrudRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public Task<OperacaoOpcao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return _context
                .Set<OperacaoOpcao>()
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public void Excluir(
            OperacaoOpcao operacaoOpcao)
        {
            _context
                .Set<OperacaoOpcao>()
                .Remove(operacaoOpcao);
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}