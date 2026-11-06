using Investimentos.Application.EstimativaIr;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Investimentos.Domain.Entities;

namespace Investimentos.Infrastructure.Persistence.Repositories;

public class EstimativaIrRepository(InvestimentosDbContext db)
{
    public async Task<(HistoricoEstimativaIr Historico, RelatorioEstimativaIr Relatorio)> SalvarAsync(
        int ano, string autor, CancellationToken ct)
    {
        var relatorio = await CalcularAsync(ano, ct);
        var ultimo = await db.HistoricosEstimativaIr.Where(x => x.Ano == ano)
            .OrderByDescending(x => x.CriadoEmUtc).FirstOrDefaultAsync(ct);
        var anterior = ultimo is null ? null : JsonSerializer.Deserialize<RelatorioEstimativaIr>(ultimo.Conteudo, JsonSerializerOptions.Web);
        string Normalizar(RelatorioEstimativaIr r) => JsonSerializer.Serialize(r with { CalculadoEmUtc = default }, JsonSerializerOptions.Web);
        if (ultimo is not null && anterior is not null && Normalizar(anterior) == Normalizar(relatorio))
            return (ultimo, anterior);
        var historico = new HistoricoEstimativaIr { Ano = ano, CriadoPor = autor,
            Conteudo = JsonSerializer.Serialize(relatorio, JsonSerializerOptions.Web) };
        db.HistoricosEstimativaIr.Add(historico);
        await db.SaveChangesAsync(ct);
        return (historico, relatorio);
    }

    public async Task<RelatorioEstimativaIr> CalcularAsync(int ano, CancellationToken ct)
    {
        var limite = new DateTime(ano + 1, 1, 1);
        var operacoes = await db.Operacoes.AsNoTracking().Include(x => x.Investidor)
            .Include(x => x.TipoOperacao).Include(x => x.Ativo).ThenInclude(x => x.TipoAtivo)
            .Where(x => x.Data < limite).OrderBy(x => x.Data).ThenBy(x => x.Sequencia).ToListAsync(ct);
        var movimentos = operacoes.Select(x => new MovimentoIr(x.Id, x.InvestidorId,
            x.Investidor.Nome, x.Ativo.Ticker.Codigo, x.Ativo.TipoAtivo.Codigo.ToUpperInvariant(),
            x.TipoOperacao.Codigo.ToUpperInvariant(), x.Data, x.Sequencia, x.Quantidade, x.PrecoUnitario, x.Taxas)).ToList();
        var opcoes = await db.OperacoesOpcoes.AsNoTracking().Include(x => x.Investidor).Include(x => x.Ativo)
            .Where(x => x.Situacao != "ABERTA" && x.DataOperacao < limite).ToListAsync(ct);
        movimentos = movimentos.Select(m => m.Tipo == "VENDA" && opcoes.Any(o =>
            o.Situacao == "EXECUTADA" && o.InvestidorId == m.InvestidorId &&
            o.Ativo.Ticker.Codigo == m.Ticker && o.DataOperacao <= m.Data)
            ? m with { Pendencia = "Conferir custo após exercício de opção no mesmo ativo; prêmio não foi integrado automaticamente." }
            : m).ToList();
        movimentos.AddRange(opcoes.Select(x => new MovimentoIr(x.Id, x.InvestidorId, x.Investidor.Nome,
            x.TickerOpcao, x.DataFinalizacao?.Date == x.DataOperacao.Date ? "DAY_TRADE" : "OPCAO",
            "OPCAO " + x.Situacao, x.DataFinalizacao ?? x.Vencimento, 0, x.Quantidade, x.PremioUnitario,
            x.Taxas, x.Situacao == "EXECUTADA" ? null : x.ResultadoFinal,
            x.Situacao == "EXECUTADA" ? "Exercício exige conciliar prêmio, custo do ativo e data fiscal; não incluído automaticamente." : null)));
        var registros = await db.RetencoesDefinitivasIr.AsNoTracking().ToListAsync(ct);
        var confirmacoes = registros.GroupBy(x => x.OperacaoId)
            .Select(x => x.OrderByDescending(y => y.CriadoEmUtc).ThenByDescending(y => y.Id).First())
            .Select(x => new ConfirmacaoIr(x.OperacaoId, x.Valor, x.Observacao));
        var relatorio = CalculadoraEstimativaIr.Calcular(ano, movimentos, confirmacoes);
        var investidores = await db.Investidores.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct);
        return relatorio with { Investidores = investidores.Select(x =>
            relatorio.Investidores.FirstOrDefault(y => y.InvestidorId == x.Id) ??
            new ResumoInvestidorIr(x.Id, x.Nome, 0, 0, 0, 0, 0)).ToList() };
    }
}
