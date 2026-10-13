using System.Text.RegularExpressions;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.ExceptionHandling;

public sealed class LogAplicacaoService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<LogAplicacaoService> _logger;

    public LogAplicacaoService(IServiceScopeFactory scopes,
        IHostEnvironment environment, ILogger<LogAplicacaoService> logger)
    {
        _scopes = scopes;
        _environment = environment;
        _logger = logger;
    }

    private static string Limpar(string texto)
    {
        // Nunca persistir credenciais que apareçam no texto de exceções HTTP.
        texto = Regex.Replace(texto, @"(?i)(Bearer\s+)\S+", "$1[REDACTED]");
        texto = Regex.Replace(texto, @"(?i)(api[_-]?key|token|password|secret)(\s*[:=]\s*)\S+", "$1$2[REDACTED]");
        return texto.Length > 12000 ? texto[..12000] : texto;
    }

    public async Task RegistrarAsync(string modulo, string operacao,
        Exception erro, string? correlacaoId = null, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InvestimentosDbContext>();
            db.LogsAplicacao.Add(new LogAplicacao
            {
                Ambiente = _environment.EnvironmentName,
                Modulo = modulo,
                Operacao = operacao,
                Nivel = "ERRO",
                Mensagem = Limpar(erro.Message)[..Math.Min(1500, Limpar(erro.Message).Length)],
                Detalhes = Limpar(erro.ToString()),
                CorrelacaoId = correlacaoId
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nao foi possivel persistir o log da aplicacao. Correlacao: {CorrelacaoId}", correlacaoId);
        }
    }

    public async Task<IReadOnlyList<LogAplicacao>> ListarAsync(
        string? modulo, string? nivel, string? correlacaoId, int limite,
        CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvestimentosDbContext>();
        var query = db.LogsAplicacao.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(modulo))
            query = query.Where(x => x.Modulo == modulo);
        if (!string.IsNullOrWhiteSpace(nivel))
            query = query.Where(x => x.Nivel == nivel);
        if (!string.IsNullOrWhiteSpace(correlacaoId))
            query = query.Where(x => x.CorrelacaoId == correlacaoId);
        return await query.OrderByDescending(x => x.DataUtc)
            .Take(Math.Clamp(limite, 1, 500)).ToListAsync(ct);
    }
}
