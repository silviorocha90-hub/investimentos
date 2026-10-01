using Investimentos.Application.Fiscal;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class FiscalRepository : IFiscalRepository
    {
        private readonly InvestimentosDbContext _context;

        public FiscalRepository(InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<FiscalResumoDto> ConsultarAsync(
            Guid? investidorId = null,
            int? ano = null,
            CancellationToken cancellationToken = default)
        {
            var opcoes = await _context.OperacoesOpcoes
                .AsNoTracking()
                .Include(x => x.Investidor)
                .Where(x => !investidorId.HasValue || x.InvestidorId == investidorId.Value)
                .ToListAsync(cancellationToken);

            var operacoesAcoes = await _context.Operacoes
                .AsNoTracking()
                .Include(x => x.Investidor)
                .Include(x => x.TipoOperacao)
                .Include(x => x.Ativo)
                    .ThenInclude(x => x.TipoAtivo)
                .Where(x => x.Ativo.TipoAtivo.Codigo == "ACAO")
                .Where(x => !investidorId.HasValue || x.InvestidorId == investidorId.Value)
                .OrderBy(x => x.Data)
                .ThenBy(x => x.Sequencia)
                .ToListAsync(cancellationToken);

            var resultadosAcoes = new List<(int Ano, int Mes, Guid InvestidorId, string Investidor, decimal Resultado, decimal Vendas)>();

            foreach (var grupoAtivo in operacoesAcoes.GroupBy(x => new { x.InvestidorId, x.AtivoId }))
            {
                decimal quantidade = 0m;
                decimal custoTotal = 0m;

                foreach (var operacao in grupoAtivo)
                {
                    var tipo = operacao.TipoOperacao.Codigo.Trim().ToUpperInvariant();

                    if (tipo == "COMPRA")
                    {
                        quantidade += operacao.Quantidade;
                        custoTotal += operacao.Quantidade * operacao.PrecoUnitario + operacao.Taxas;
                        continue;
                    }

                    if (tipo != "VENDA" || quantidade <= 0 || operacao.Quantidade > quantidade)
                        continue;

                    var precoMedio = custoTotal / quantidade;
                    var custoVendido = operacao.Quantidade * precoMedio;
                    var valorVenda = operacao.Quantidade * operacao.PrecoUnitario;
                    var resultado = valorVenda - custoVendido - operacao.Taxas;

                    resultadosAcoes.Add((
                        operacao.Data.Year,
                        operacao.Data.Month,
                        operacao.InvestidorId,
                        operacao.Investidor.Nome,
                        resultado,
                        valorVenda));

                    custoTotal -= custoVendido;
                    quantidade -= operacao.Quantidade;
                    if (quantidade == 0) custoTotal = 0m;
                }
            }

            var darfs = await _context.DescontosFiscais
                .AsNoTracking()
                .Include(x => x.Investidor)
                .Where(x => x.Tipo == "DARF" || x.Tipo == "DARF_SPRAD")
                .Where(x => !investidorId.HasValue || x.InvestidorId == investidorId.Value)
                .ToListAsync(cancellationToken);

            var itens = opcoes
                .Where(x => x.DataFinalizacao.HasValue && x.ResultadoFinal.HasValue)
                .Where(x => !ano.HasValue || x.DataFinalizacao!.Value.Year == ano.Value)
                .Select(x =>
                {
                    var data = x.DataFinalizacao!.Value;
                    var dayTrade = data.Date == x.DataOperacao.Date;
                    var resultado = x.ResultadoFinal!.Value;

                    return new
                    {
                        data.Year,
                        data.Month,
                        x.InvestidorId,
                        Investidor = x.Investidor.Nome,
                        Comum = dayTrade ? 0m : resultado,
                        DayTrade = dayTrade ? resultado : 0m
                    };
                })
                .ToList();

            if (ano.HasValue)
            {
                darfs = darfs.Where(x => x.DataPagamento.AddMonths(-1).Year == ano.Value).ToList();
                resultadosAcoes = resultadosAcoes.Where(x => x.Ano == ano.Value).ToList();
            }

            var chaves = itens
                .Select(x => new
                {
                    Ano = x.Year,
                    Mes = x.Month,
                    InvestidorId = (Guid?)x.InvestidorId,
                    Investidor = x.Investidor
                })
                .Concat(resultadosAcoes.Select(x => new
                {
                    x.Ano,
                    x.Mes,
                    InvestidorId = (Guid?)x.InvestidorId,
                    x.Investidor
                }))
                .Concat(darfs.Select(x => new
                {
                    Ano = x.DataPagamento.AddMonths(-1).Year,
                    Mes = x.DataPagamento.AddMonths(-1).Month,
                    InvestidorId = x.InvestidorId,
                    Investidor = x.Investidor?.Nome ?? "Não atribuído"
                }))
                .Distinct()
                .OrderByDescending(x => x.Ano)
                .ThenByDescending(x => x.Mes)
                .ThenBy(x => x.Investidor)
                .ToList();

            var meses = chaves.Select(chave =>
            {
                var grupo = itens.Where(x => x.Year == chave.Ano && x.Month == chave.Mes && (Guid?)x.InvestidorId == chave.InvestidorId).ToList();
                var pago = darfs.Where(x =>
                    x.DataPagamento.AddMonths(-1).Year == chave.Ano &&
                    x.DataPagamento.AddMonths(-1).Month == chave.Mes &&
                    x.InvestidorId == chave.InvestidorId).Sum(x => x.Valor);
                var acoesMes = resultadosAcoes
                    .Where(x => x.Ano == chave.Ano && x.Mes == chave.Mes && (Guid?)x.InvestidorId == chave.InvestidorId)
                    .ToList();

                var resultadoOpcoesComum = grupo.Sum(x => x.Comum);
                var resultadoDayTrade = grupo.Sum(x => x.DayTrade);
                var resultadoAcoes = acoesMes.Sum(x => x.Resultado);
                var vendasAcoes = acoesMes.Sum(x => x.Vendas);

                // Aproxima a apuração do MyProfit: ações e opções comuns compõem
                // a mesma base mensal; day trade permanece separado.
                // Vendas de ações até R$ 20 mil no mês não entram na base tributável.
                var resultadoAcoesTributavel = vendasAcoes > 20_000m ? resultadoAcoes : 0m;
                var baseComum = Math.Max(0m, resultadoOpcoesComum + resultadoAcoesTributavel);
                var baseDayTrade = Math.Max(0m, resultadoDayTrade);
                var estimado =
                    decimal.Round(baseComum * 15m / 100m, 2, MidpointRounding.AwayFromZero) +
                    decimal.Round(baseDayTrade * 20m / 100m, 2, MidpointRounding.AwayFromZero);

                return new FiscalMesDto(
                    chave.Ano, chave.Mes, chave.InvestidorId, chave.Investidor,
                    resultadoOpcoesComum + resultadoAcoesTributavel, resultadoDayTrade,
                    estimado, pago, estimado - pago, grupo.Count + acoesMes.Count, 0);
            }).ToList();

            var pendencias = opcoes.Count(x =>
                x.Situacao == "EXECUTADA" &&
                x.ValorExecucao.HasValue &&
                !x.DataFinalizacao.HasValue);

            var avisos = new List<string>
            {
                "Estimativa de opções é informativa e não substitui a apuração fiscal oficial.",
                "Prejuízos, compensações, retenções e regras fiscais de ações não são compensados automaticamente."
            };

            if (darfs.Any(x => !x.InvestidorId.HasValue))
                avisos.Add("Existem DARFs sem investidor atribuído.");

            if (pendencias > 0)
                avisos.Add("Existem opções exercidas sem data de finalização e fora da estimativa.");

            return new FiscalResumoDto(
                meses.Sum(x => x.ResultadoComumOpcoes),
                meses.Sum(x => x.ResultadoDayTradeOpcoes),
                meses.Sum(x => x.IrEstimadoOpcoes),
                meses.Sum(x => x.DarfPago),
                meses.Sum(x => x.DiferencaEstimadoPago),
                meses.Sum(x => x.OperacoesConsideradas),
                pendencias,
                meses,
                avisos);
        }
    }
}
