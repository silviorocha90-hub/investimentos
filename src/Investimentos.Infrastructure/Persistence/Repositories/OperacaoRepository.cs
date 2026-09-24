using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class OperacaoRepository : IOperacaoRepository
    {
        private readonly InvestimentosDbContext _context;

        public OperacaoRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(
            Operacao operacao,
            CancellationToken cancellationToken = default)
        {
            await _context.Operacoes.AddAsync(
                operacao,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<Operacao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Operacoes
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public void Remover(
            Operacao operacao)
        {
            _context.Operacoes.Remove(operacao);
        }

        public Task SalvarAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<Investidor?> ObterInvestidorPorIdAsync(
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
            var tickerValueObject =
                new Ticker(ticker);

            return await _context.Ativos
                .Include(x => x.TipoAtivo)
                .ThenInclude(x => x.ClasseAtivo)
                .FirstOrDefaultAsync(
                    x => x.Ticker == tickerValueObject,
                    cancellationToken);
        }

        public async Task<TipoOperacao?> ObterTipoOperacaoPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default)
        {
            return await _context.TiposOperacoes
                .FirstOrDefaultAsync(
                    x =>
                        x.Codigo == codigo &&
                        x.Ativo,
                    cancellationToken);
        }

        public async Task<int> ObterProximaSequenciaAsync(
            Guid investidorId,
            DateTime data,
            CancellationToken cancellationToken = default)
        {
            var inicio =
                data.Date;

            var fim =
                inicio.AddDays(1);

            var ultimaSequencia =
                await _context.Operacoes
                    .Where(x =>
                        x.InvestidorId == investidorId &&
                        x.Data >= inicio &&
                        x.Data < fim)
                    .MaxAsync(
                        x => (int?)x.Sequencia,
                        cancellationToken)
                ?? 0;

            return ultimaSequencia + 1;
        }

        public async Task<decimal> ObterQuantidadeDisponivelAsync(
            Guid investidorId,
            Guid ativoId,
            DateTime data,
            int sequencia,
            CancellationToken cancellationToken = default)
        {
            var operacoes =
                await _context.Operacoes
                    .AsNoTracking()
                    .Where(x =>
                        x.InvestidorId == investidorId &&
                        x.AtivoId == ativoId &&
                        (
                            x.Data < data ||
                            (
                                x.Data == data &&
                                x.Sequencia < sequencia
                            )
                        ))
                    .Select(x => new
                    {
                        TipoOperacao =
                            x.TipoOperacao.Codigo,

                        x.Quantidade
                    })
                    .ToListAsync(
                        cancellationToken);

            decimal quantidadeDisponivel = 0;

            foreach (var operacao in operacoes)
            {
                if (operacao.TipoOperacao == "COMPRA")
                {
                    quantidadeDisponivel +=
                        operacao.Quantidade;
                }
                else if (operacao.TipoOperacao == "VENDA")
                {
                    quantidadeDisponivel -=
                        operacao.Quantidade;
                }
            }

            return quantidadeDisponivel;
        }

        public async Task<bool> HistoricoPermaneceValidoSemOperacaoAsync(
            Guid operacaoId,
            Guid investidorId,
            Guid ativoId,
            CancellationToken cancellationToken = default)
        {
            var operacoes =
                await _context.Operacoes
                    .AsNoTracking()
                    .Where(x =>
                        x.Id != operacaoId &&
                        x.InvestidorId == investidorId &&
                        x.AtivoId == ativoId)
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .Select(x => new
                    {
                        TipoOperacao = x.TipoOperacao.Codigo,
                        x.Quantidade
                    })
                    .ToListAsync(
                        cancellationToken);

            decimal quantidadeAcumulada = 0;

            foreach (var operacao in operacoes)
            {
                if (operacao.TipoOperacao == "COMPRA")
                {
                    quantidadeAcumulada +=
                        operacao.Quantidade;
                }
                else if (operacao.TipoOperacao == "VENDA")
                {
                    quantidadeAcumulada -=
                        operacao.Quantidade;
                }

                if (quantidadeAcumulada < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}