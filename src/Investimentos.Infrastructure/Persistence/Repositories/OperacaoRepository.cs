using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
                .Include(x => x.TipoOperacao)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public void Remover(
            Operacao operacao)
        {
            _context.Operacoes.Remove(operacao);
        }

        public async Task SalvarAsync(
            CancellationToken cancellationToken = default)
        {
            _context.ChangeTracker.DetectChanges();
            var transferencias = _context.ChangeTracker.Entries<Operacao>()
                .Where(x => x.State == EntityState.Modified &&
                    x.Property(o => o.InvestidorId).OriginalValue != x.Entity.InvestidorId)
                .ToList();

            if (transferencias.Count == 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            // Inclui a transação na estratégia de repetição configurada para SQL Server.
            var estrategia = _context.Database.CreateExecutionStrategy();
            await estrategia.ExecuteAsync(async () =>
            {
                await using var transacao = _context.Database.IsRelational()
                    ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    : null;

                foreach (var entrada in transferencias)
                {
                    var operacao = entrada.Entity;
                    var sequencia = await ObterProximaSequenciaAsync(
                        operacao.InvestidorId, operacao.Data, cancellationToken);
                    operacao.Atualizar(operacao.Data, operacao.Quantidade,
                        operacao.PrecoUnitario, operacao.Taxas, sequencia: sequencia);
                    await ValidarTransferenciaAsync(operacao,
                        entrada.Property(o => o.InvestidorId).OriginalValue, cancellationToken);
                }

                // Só aceita os novos valores após confirmar a transação.
                await _context.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);
                if (transacao is not null) await transacao.CommitAsync(cancellationToken);
            });
            _context.ChangeTracker.AcceptAllChanges();
        }

        private async Task ValidarTransferenciaAsync(
            Operacao operacao, Guid investidorOrigemId, CancellationToken cancellationToken)
        {
            var investidores = new[] { investidorOrigemId, operacao.InvestidorId };
            var historico = await _context.Operacoes.AsNoTracking()
                .Where(x => x.Id != operacao.Id && x.AtivoId == operacao.AtivoId &&
                    investidores.Contains(x.InvestidorId))
                .Select(x => new
                {
                    x.InvestidorId, x.Data, x.Sequencia,
                    TipoOperacao = x.TipoOperacao.Codigo, x.Quantidade
                })
                .ToListAsync(cancellationToken);
            historico.Add(new
            {
                operacao.InvestidorId, operacao.Data, operacao.Sequencia,
                TipoOperacao = operacao.TipoOperacao.Codigo, operacao.Quantidade
            });

            foreach (var investidorId in investidores.Distinct())
            {
                decimal quantidade = 0;
                foreach (var item in historico.Where(x => x.InvestidorId == investidorId)
                    .OrderBy(x => x.Data).ThenBy(x => x.Sequencia))
                {
                    if (item.TipoOperacao == "COMPRA") quantidade += item.Quantidade;
                    else if (item.TipoOperacao == "VENDA") quantidade -= item.Quantidade;
                    if (quantidade < 0)
                    {
                        var carteira = investidorId == investidorOrigemId ? "origem" : "destino";
                        throw new InvalidOperationException(
                            $"A troca de investidor deixaria uma venda sem quantidade suficiente na carteira de {carteira}. " +
                            "Ajuste as operações relacionadas antes de transferir este lançamento.");
                    }
                }
            }
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