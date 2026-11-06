using Investimentos.Api.AssistenteIa;
using Investimentos.Infrastructure.Persistence;
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Startup;

// Preserva versões anuais mesmo quando ninguém abre a Administração.
public class EstimativaIrWorker(IServiceScopeFactory scopes, ILogger<EstimativaIrWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InvestimentosDbContext>();
                var repo = scope.ServiceProvider.GetRequiredService<EstimativaIrRepository>();
                var atual = AssistenteIaWorker.ObterAgoraSaoPaulo().Year;
                var anos = await db.Operacoes.Select(x => x.Data.Year).Distinct().ToListAsync(stoppingToken);
                anos.AddRange(await db.OperacoesOpcoes.Select(x => (x.DataFinalizacao ?? x.Vencimento).Year).Distinct().ToListAsync(stoppingToken));
                anos.AddRange(await db.HistoricosEstimativaIr.Select(x => x.Ano).Distinct().ToListAsync(stoppingToken));
                anos.Add(atual);
                foreach (var ano in anos.Where(x => x >= 2000 && x <= atual).Distinct().Order())
                    await repo.SalvarAsync(ano, "rotina-automatica", stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Falha ao preservar histórico anual de estimativa de IR; nova tentativa em 15 minutos."); }
            try { await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
