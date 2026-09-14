using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class CarteiraRepository
        : ICarteiraRepository
    {
        private readonly InvestimentosDbContext _context;

        public CarteiraRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<OperacaoCarteiraDto>>
            ObterOperacoesAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            var operacoes =
                await _context.Operacoes
                    .AsNoTracking()
                    .Where(x =>
                        x.InvestidorId == investidorId)
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .Select(x =>
                        new OperacaoCarteiraDto(
                            x.Id,
                            x.AtivoId,
                            x.Ativo.Ticker.Codigo,
                            x.Ativo.Nome,
                            x.TipoOperacao.Codigo,
                            x.Quantidade,
                            x.PrecoUnitario,
                            x.Taxas,
                            x.Data,
                            x.Sequencia)
                        {
                            TipoAtivoCodigo =
                                x.Ativo.TipoAtivo.Codigo,

                            TipoAtivoNome =
                                x.Ativo.TipoAtivo.Nome
                        })
                    .ToListAsync(
                        cancellationToken);

            return operacoes;
        }
    }
}