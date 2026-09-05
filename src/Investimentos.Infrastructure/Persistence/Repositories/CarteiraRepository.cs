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

        public async Task<IReadOnlyList<PosicaoAtivoDto>>
            ObterPosicoesAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            var operacoes =
                await _context.Operacoes
                    .AsNoTracking()
                    .Where(x =>
                        x.InvestidorId == investidorId)
                    .Select(x => new
                    {
                        x.AtivoId,
                        Ticker = x.Ativo.Ticker.Codigo,
                        x.Ativo.Nome,
                        TipoOperacao =
                            x.TipoOperacao.Codigo,
                        x.Quantidade,
                        x.PrecoUnitario,
                        x.Taxas,
                        x.Data,
                        x.Sequencia
                    })
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .ToListAsync(cancellationToken);

            var posicoes =
                operacoes
                    .GroupBy(x => new
                    {
                        x.AtivoId,
                        x.Ticker,
                        x.Nome
                    })
                    .Select(grupo =>
                    {
                        decimal quantidade = 0;
                        decimal custoTotal = 0;

                        foreach (var operacao in grupo)
                        {
                            if (operacao.TipoOperacao ==
                                "COMPRA")
                            {
                                quantidade +=
                                    operacao.Quantidade;

                                custoTotal +=
                                    (operacao.Quantidade *
                                     operacao.PrecoUnitario)
                                    + operacao.Taxas;
                            }
                            else if (
                                operacao.TipoOperacao ==
                                "VENDA")
                            {
                                if (quantidade <= 0)
                                {
                                    continue;
                                }

                                var precoMedio =
                                    custoTotal /
                                    quantidade;

                                var quantidadeVendida =
                                    Math.Min(
                                        operacao.Quantidade,
                                        quantidade);

                                custoTotal -=
                                    quantidadeVendida *
                                    precoMedio;

                                quantidade -=
                                    quantidadeVendida;
                            }
                        }

                        var precoMedioFinal =
                            quantidade > 0
                                ? custoTotal /
                                  quantidade
                                : 0;

                        return new PosicaoAtivoDto(
                            grupo.Key.Ticker,
                            grupo.Key.Nome,
                            quantidade,
                            precoMedioFinal,
                            custoTotal);
                    })
                    .Where(x =>
                        x.Quantidade > 0)
                    .OrderBy(x =>
                        x.Ticker)
                    .ToList();

            return posicoes;
        }
    }
}