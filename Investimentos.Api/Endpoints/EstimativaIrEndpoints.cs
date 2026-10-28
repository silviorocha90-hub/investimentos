using System.Security.Claims;
using System.Text.Json;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Endpoints;

public static class EstimativaIrEndpoints
{
    public static void MapEstimativaIrEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/admin/estimativa-ir").RequireAuthorization(p => p.RequireRole("Admin"));
        grupo.MapGet("/anos", async (InvestimentosDbContext db, CancellationToken ct) =>
        {
            var anos = await db.Operacoes.Select(x => x.Data.Year).Distinct().ToListAsync(ct);
            anos.AddRange(await db.OperacoesOpcoes.Select(x => (x.DataFinalizacao ?? x.Vencimento).Year).Distinct().ToListAsync(ct));
            anos.AddRange(await db.HistoricosEstimativaIr.Select(x => x.Ano).Distinct().ToListAsync(ct));
            anos.Add(AssistenteIa.AssistenteIaWorker.ObterAgoraSaoPaulo().Year);
            return Results.Ok(anos.Distinct().OrderDescending());
        });
        // Cada atualização explícita cria uma versão imutável; nunca sobrescreve anos anteriores.
        grupo.MapPost("/{ano:int}/atualizar", async (int ano, ClaimsPrincipal usuario,
            InvestimentosDbContext db, EstimativaIrRepository repo, CancellationToken ct) =>
        {
            if (ano < 2000 || ano > 2100) return Results.BadRequest(new { detail = "Ano inválido." });
            var (historico, relatorio) = await repo.SalvarAsync(ano,
                usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? "", ct);
            return Results.Ok(new { historico.Id, historico.CriadoEmUtc, relatorio });
        });
        grupo.MapGet("/{ano:int}/historico", async (int ano, InvestimentosDbContext db, CancellationToken ct) =>
            Results.Ok(await db.HistoricosEstimativaIr.AsNoTracking().Where(x => x.Ano == ano)
                .OrderByDescending(x => x.CriadoEmUtc)
                .Select(x => new { x.Id, x.Ano, x.CriadoEmUtc }).ToListAsync(ct)));
        grupo.MapGet("/historico/{id:guid}", async (Guid id, InvestimentosDbContext db, CancellationToken ct) =>
        {
            var registro = await db.HistoricosEstimativaIr.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return registro is null ? Results.NotFound() : Results.Content(registro.Conteudo, "application/json");
        });
        grupo.MapPost("/retencao/{operacaoId:guid}", async (Guid operacaoId, RetencaoIrRequest request,
            ClaimsPrincipal usuario, InvestimentosDbContext db, CancellationToken ct) =>
        {
            if (request.Valor < 0 || request.Valor > 999999999 || decimal.Round(request.Valor, 2) != request.Valor ||
                string.IsNullOrWhiteSpace(request.Observacao) || request.Observacao.Length > 1000)
                return Results.BadRequest(new { detail = "Informe valor não negativo com até 2 casas e observação de até 1.000 caracteres." });
            var operacao = await db.Operacoes.Include(x => x.TipoOperacao).Include(x => x.Ativo)
                .ThenInclude(x => x.TipoAtivo).FirstOrDefaultAsync(x => x.Id == operacaoId, ct);
            if (operacao is null || operacao.TipoOperacao.Codigo != "VENDA")
                return Results.BadRequest(new { detail = "Selecione uma venda existente." });
            if (new[] { "ACAO", "FII", "ETF_RV", "BDR" }.Contains(operacao.Ativo.TipoAtivo.Codigo.ToUpperInvariant()))
                return Results.BadRequest(new { detail = "Este campo é para retenção definitiva, não IRRF antecipado de renda variável." });
            db.RetencoesDefinitivasIr.Add(new RetencaoDefinitivaIr { OperacaoId = operacaoId,
                Valor = request.Valor, Observacao = request.Observacao.Trim(),
                CriadoPor = usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? "" });
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
public record RetencaoIrRequest(decimal Valor, string Observacao);
