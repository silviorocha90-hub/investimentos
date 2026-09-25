using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class HistoricoPatrimonioRepository :
        IHistoricoPatrimonioRepository
    {
        private readonly InvestimentosDbContext _context;

        public HistoricoPatrimonioRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<HistoricoPatrimonio>> ListarAsync(
            Guid? investidorId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.HistoricosPatrimonio
                .AsNoTracking()
                .Include(x => x.Investidor)
                .AsQueryable();

            if (investidorId.HasValue)
            {
                query = query.Where(x => x.InvestidorId == investidorId.Value);
            }

            return await query
                .OrderBy(x => x.DataReferencia)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> ObterTotalAtualAsync(
            CancellationToken cancellationToken = default)
        {
            var patrimonios = await _context.HistoricosPatrimonio
                .AsNoTracking()
                .GroupBy(x => x.InvestidorId)
                .Select(grupo => grupo
                    .OrderByDescending(x => x.DataReferencia)
                    .Select(x => x.ValorCarteira)
                    .First())
                .ToListAsync(cancellationToken);

            return patrimonios.Sum();
        }

        public async Task<IReadOnlyList<EvolucaoInvestidorDto>>
            ListarEvolucaoAsync(
                CancellationToken cancellationToken = default)
        {
            var registros = await _context.HistoricosPatrimonio
                .AsNoTracking()
                .Include(x => x.Investidor)
                .OrderBy(x => x.Investidor.Nome)
                .ThenBy(x => x.DataReferencia)
                .Select(x => new
                {
                    x.Investidor.Nome,
                    x.DataReferencia,
                    x.ValorCarteira
                })
                .ToListAsync(cancellationToken);

            return registros
                .GroupBy(x => x.Nome)
                .Select(grupo => new EvolucaoInvestidorDto(
                    grupo.Key,
                    grupo.Select(x => new EvolucaoPontoDto(
                        x.DataReferencia,
                        x.ValorCarteira))
                        .ToList()))
                .ToList();
        }

        public Task<HistoricoPatrimonio?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _context.HistoricosPatrimonio
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task AdicionarAsync(
            HistoricoPatrimonio historicoPatrimonio,
            CancellationToken cancellationToken = default)
        {
            await _context.HistoricosPatrimonio.AddAsync(
                historicoPatrimonio,
                cancellationToken);
        }

        public void Excluir(
            HistoricoPatrimonio historicoPatrimonio)
        {
            _context.HistoricosPatrimonio.Remove(
                historicoPatrimonio);
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}