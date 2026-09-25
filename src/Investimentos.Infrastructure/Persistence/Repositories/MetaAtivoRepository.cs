using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Metas;
using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class MetaAtivoRepository : IMetaAtivoRepository
    {
        private readonly InvestimentosDbContext _context;
        private readonly CalcularCarteiraService _calcularCarteira;

        public MetaAtivoRepository(
            InvestimentosDbContext context,
            CalcularCarteiraService calcularCarteira)
        {
            _context = context;
            _calcularCarteira = calcularCarteira;
        }

        public async Task<IReadOnlyList<MetaAtivoDto>> ListarAsync(
            Guid? investidorId = null,
            CancellationToken cancellationToken = default)
        {
            var metas = await _context.MetasAtivos
                .AsNoTracking()
                .Include(x => x.Investidor)
                .Include(x => x.Ativo)
                .Where(x => !investidorId.HasValue ||
                            x.InvestidorId == investidorId.Value)
                .ToListAsync(cancellationToken);

            var resultado = new List<MetaAtivoDto>();

            foreach (var grupo in metas.GroupBy(x => x.InvestidorId))
            {
                var operacoes = await _context.Operacoes
                    .AsNoTracking()
                    .Where(x => x.InvestidorId == grupo.Key)
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .Select(x => new OperacaoCarteiraDto(
                        x.Id,
                        x.AtivoId,
                        x.Ativo.Ticker.Codigo,
                        x.Ativo.Nome,
                        x.TipoOperacao.Codigo,
                        x.Quantidade,
                        x.PrecoUnitario,
                        x.Taxas,
                        x.Data,
                        x.Sequencia))
                    .ToListAsync(cancellationToken);

                var posicoes = _calcularCarteira
                    .Calcular(operacoes)
                    .ToDictionary(x => x.Ticker, StringComparer.OrdinalIgnoreCase);

                foreach (var meta in grupo)
                {
                    var atual = posicoes.TryGetValue(
                        meta.Ativo.Ticker.Codigo,
                        out var posicao)
                            ? Math.Max(posicao.Quantidade, 0)
                            : 0;

                    resultado.Add(Mapear(meta, atual));
                }
            }

            return resultado
                .OrderBy(x => x.Investidor)
                .ThenBy(x => x.Ticker)
                .ToList();
        }

        public async Task<MetaAtivoDto> SalvarAsync(
            SalvarMetaAtivoRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.QuantidadeDesejada <= 0)
                throw new ArgumentException("A quantidade desejada deve ser maior que zero.");

            var investidor = await _context.Investidores
                .FirstOrDefaultAsync(x => x.Id == request.InvestidorId, cancellationToken)
                ?? throw new ArgumentException("Investidor não encontrado.");

            var ativo = await _context.Ativos
                .FirstOrDefaultAsync(x => x.Id == request.AtivoId, cancellationToken)
                ?? throw new ArgumentException("Ativo não encontrado.");

            var meta = await _context.MetasAtivos
                .FirstOrDefaultAsync(
                    x => x.InvestidorId == request.InvestidorId &&
                         x.AtivoId == request.AtivoId,
                    cancellationToken);

            if (meta is null)
            {
                meta = new MetaAtivo(
                    investidor,
                    ativo,
                    request.QuantidadeDesejada);
                _context.MetasAtivos.Add(meta);
            }
            else
            {
                meta.Atualizar(request.QuantidadeDesejada);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var quantidadeAtual = await ObterQuantidadeAtualAsync(
                request.InvestidorId,
                ativo.Id,
                cancellationToken);

            return Mapear(meta, quantidadeAtual);
        }

        public async Task<bool> ExcluirAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var meta = await _context.MetasAtivos
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (meta is null)
                return false;

            _context.MetasAtivos.Remove(meta);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task<decimal> ObterQuantidadeAtualAsync(
            Guid investidorId,
            Guid ativoId,
            CancellationToken cancellationToken)
        {
            var operacoes = await _context.Operacoes
                .AsNoTracking()
                .Where(x => x.InvestidorId == investidorId)
                .OrderBy(x => x.Data)
                .ThenBy(x => x.Sequencia)
                .Select(x => new OperacaoCarteiraDto(
                    x.Id,
                    x.AtivoId,
                    x.Ativo.Ticker.Codigo,
                    x.Ativo.Nome,
                    x.TipoOperacao.Codigo,
                    x.Quantidade,
                    x.PrecoUnitario,
                    x.Taxas,
                    x.Data,
                    x.Sequencia))
                .ToListAsync(cancellationToken);

            var ticker = await _context.Ativos
                .AsNoTracking()
                .Where(x => x.Id == ativoId)
                .Select(x => x.Ticker.Codigo)
                .FirstAsync(cancellationToken);

            return Math.Max(
                _calcularCarteira.Calcular(operacoes)
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Ticker,
                            ticker,
                            StringComparison.OrdinalIgnoreCase))
                    ?.Quantidade ?? 0,
                0);
        }

        private static MetaAtivoDto Mapear(
            MetaAtivo meta,
            decimal quantidadeAtual)
        {
            var faltante = Math.Max(
                meta.QuantidadeDesejada - quantidadeAtual,
                0);

            var percentual = meta.QuantidadeDesejada > 0
                ? quantidadeAtual / meta.QuantidadeDesejada * 100m
                : 0;

            return new MetaAtivoDto(
                meta.Id,
                meta.InvestidorId,
                meta.Investidor.Nome,
                meta.AtivoId,
                meta.Ativo.Ticker.Codigo,
                meta.Ativo.Nome,
                quantidadeAtual,
                meta.QuantidadeDesejada,
                faltante,
                percentual);
        }
    }
}
