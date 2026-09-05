using Investimentos.Application.Interfaces;
using Investimentos.Application.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class HistoricoPatrimonioRepository : IHistoricoPatrimonioRepository
    {
        private readonly InvestimentosDbContext _context;

        public HistoricoPatrimonioRepository(InvestimentosDbContext context)
        {
            _context = context;
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
                        x.ValorCarteira)).ToList()))
                .ToList();
        }
    }
}
