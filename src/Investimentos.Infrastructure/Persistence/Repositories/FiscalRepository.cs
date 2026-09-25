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
                    var estimado = resultado > 0
                        ? decimal.Round(resultado * (dayTrade ? 20m : 15m) / 100m, 2, MidpointRounding.AwayFromZero)
                        : 0m;

                    return new
                    {
                        data.Year,
                        data.Month,
                        x.InvestidorId,
                        Investidor = x.Investidor.Nome,
                        Comum = dayTrade ? 0m : resultado,
                        DayTrade = dayTrade ? resultado : 0m,
                        Estimado = estimado
                    };
                })
                .ToList();

            if (ano.HasValue)
                darfs = darfs.Where(x => x.DataPagamento.Year == ano.Value).ToList();

            var chaves = itens
                .Select(x => new
                {
                    Ano = x.Year,
                    Mes = x.Month,
                    InvestidorId = (Guid?)x.InvestidorId,
                    Investidor = x.Investidor
                })
                .Concat(darfs.Select(x => new
                {
                    Ano = x.DataPagamento.Year,
                    Mes = x.DataPagamento.Month,
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
                var grupo = itens.Where(x => x.Ano == chave.Ano && x.Month == chave.Mes && (Guid?)x.InvestidorId == chave.InvestidorId).ToList();
                var pago = darfs.Where(x => x.DataPagamento.Year == chave.Ano && x.DataPagamento.Month == chave.Mes && x.InvestidorId == chave.InvestidorId).Sum(x => x.Valor);
                var estimado = grupo.Sum(x => x.Estimado);

                return new FiscalMesDto(
                    chave.Ano, chave.Mes, chave.InvestidorId, chave.Investidor,
                    grupo.Sum(x => x.Comum), grupo.Sum(x => x.DayTrade),
                    estimado, pago, estimado - pago, grupo.Count, 0);
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
