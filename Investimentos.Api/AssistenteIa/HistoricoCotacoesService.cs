using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.AssistenteIa;

/// <summary>Backfill idempotente das cotacoes diarias desde o inicio das operacoes BTG.</summary>
public sealed class HistoricoCotacoesService
{
    public static readonly DateTime Inicio = new(2026, 5, 1);
    private static readonly SemaphoreSlim Trava = new(1, 1);
    private readonly InvestimentosDbContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly ILogger<HistoricoCotacoesService> _logger;

    public HistoricoCotacoesService(InvestimentosDbContext db, IHttpClientFactory http,
        IConfiguration config, ILogger<HistoricoCotacoesService> logger)
    {
        _db = db;
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<HistoricoCotacoesResultado> ImportarAsync(CancellationToken ct)
    {
        await Trava.WaitAsync(ct);
        try
        {
            var fim = AssistenteIaWorker.ObterAgoraSaoPaulo().Date;
            var ativos = await _db.Ativos.AsNoTracking().ToListAsync(ct);
            var elegiveis = ativos.Where(a =>
                System.Text.RegularExpressions.Regex.IsMatch(a.Ticker.Codigo,
                    @"^[A-Z]{4}[0-9]{1,2}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                .ToList();
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(45);
            var token = _config["BRAPI_TOKEN"];
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            var inseridos = 0;
            var atualizados = 0;
            var falhas = new List<string>();
            foreach (var ativo in elegiveis)
            {
                ct.ThrowIfCancellationRequested();
                var ticker = ativo.Ticker.Codigo;
                try
                {
                    // Endpoint brapi documentado: startDate/endDate + interval=1d.
                    var url = "https://brapi.dev/api/quote/" + Uri.EscapeDataString(ticker) +
                        "?startDate=" + Inicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                        "&endDate=" + fim.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                        "&interval=1d";
                    using var response = await client.GetAsync(url, ct);
                    if (!response.IsSuccessStatusCode)
                    {
                        falhas.Add(ticker + ": HTTP " + (int)response.StatusCode);
                        continue;
                    }
                    await using var stream = await response.Content.ReadAsStreamAsync(ct);
                    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                    if (!doc.RootElement.TryGetProperty("results", out var results) ||
                        results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0 ||
                        !results[0].TryGetProperty("historicalDataPrice", out var pontos) ||
                        pontos.ValueKind != JsonValueKind.Array)
                    {
                        falhas.Add(ticker + ": historico nao retornado");
                        continue;
                    }
                    var novos = new Dictionary<DateTime, decimal>();
                    foreach (var ponto in pontos.EnumerateArray())
                    {
                        if (!ponto.TryGetProperty("date", out var data) ||
                            !data.TryGetInt64(out var epoch)) continue;
                        var dia = DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime.Date;
                        if (dia < Inicio || dia > fim) continue;
                        if (!ponto.TryGetProperty("close", out var fechamento) ||
                            !fechamento.TryGetDecimal(out var preco) || preco <= 0) continue;
                        novos[dia] = preco;
                    }
                    var existentes = await _db.CotacoesAtivos
                        .Where(c => c.AtivoId == ativo.Id && c.DataReferencia >= Inicio &&
                            c.DataReferencia < fim.AddDays(1))
                        .ToListAsync(ct);
                    var porDia = existentes.GroupBy(c => c.DataReferencia.Date)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.DataReferencia).First());
                    foreach (var (dia, preco) in novos)
                    {
                        if (porDia.TryGetValue(dia, out var existente))
                        {
                            // Preserva a cotacao ja coletada hoje pelo fluxo normal.
                            if (dia == fim) continue;
                            if (existente.Preco != preco)
                            {
                                existente.Atualizar(existente.DataReferencia, preco);
                                atualizados++;
                            }
                        }
                        else
                        {
                            _db.CotacoesAtivos.Add(new CotacaoAtivo(ativo, dia, preco));
                            inseridos++;
                        }
                    }
                    await _db.SaveChangesAsync(ct);
                    _db.ChangeTracker.Clear();
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    _db.ChangeTracker.Clear();
                    falhas.Add(ticker + ": " + ex.GetType().Name);
                    _logger.LogWarning(ex, "Falha ao importar historico de {Ticker}", ticker);
                }
            }
            return new HistoricoCotacoesResultado(Inicio, fim, elegiveis.Count, inseridos, atualizados, falhas);
        }
        finally { Trava.Release(); }
    }
}

public sealed record HistoricoCotacoesResultado(
    DateTime Inicio, DateTime Fim, int AtivosConsultados,
    int Inseridos, int Atualizados, IReadOnlyList<string> Falhas);
