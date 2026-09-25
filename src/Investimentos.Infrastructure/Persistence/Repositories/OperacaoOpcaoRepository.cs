using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class OperacaoOpcaoRepository : IOperacaoOpcaoRepository
    {
        private readonly InvestimentosDbContext _context;

        public OperacaoOpcaoRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(
            OperacaoOpcao operacaoOpcao,
            CancellationToken cancellationToken = default)
        {
            await _context.OperacoesOpcoes.AddAsync(
                operacaoOpcao,
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
            return await _context.Ativos
                .FirstOrDefaultAsync(
                    x => x.Ticker == new Ticker(ticker),
                    cancellationToken);
        }

        public async Task<IReadOnlyList<OperacaoOpcao>> ListarAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.OperacoesOpcoes
                .AsNoTracking()
                .Include(x => x.Ativo)
                    .ThenInclude(x => x.TipoAtivo)
                .Where(x =>
                    x.InvestidorId == investidorId)
                .OrderByDescending(x =>
                    x.DataOperacao)
                .ToListAsync(
                    cancellationToken);
        }
    }
}