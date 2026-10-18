using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.AssistenteIa;

/// <summary>Importacao idempotente limitada aos ultimos 90 dias permitidos pela Brapi.</summary>
public sealed class HistoricoCotacoesService
{
    public static readonly DateTime InicioHistorico = new(2026, 5, 1);
    private const int LimiteDiasBrapi = 90;
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
            // Janela inclusiva de 90 datas (89 dias de diferenca entre inicio e fim).
            var inicio = new[] { InicioHistorico, fim.AddDays(-(LimiteDiasBrapi - 1)) }.Max();
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
            var duplicatasRemovidas = 0;
            var totalPersistido = 0;
            foreach (var ativo in elegiveis)
            {
                ct.ThrowIfCancellationRequested();
                var ticker = ativo.Ticker.Codigo;
                try
                {
                    // Endpoint brapi documentado: startDate/endDate + interval=1d.
                    var url = "https://brapi.dev/api/v2/stocks/historical?symbols=" + Uri.EscapeDataString(ticker) +
                        "&startDate=" + inicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                        "&endDate=" + fim.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                        "&interval=1d";
                    using var response = await client.GetAsync(url, ct);
                    if (!response.IsSuccessStatusCode)
                    {
                        var corpoErro = await response.Content.ReadAsStringAsync(ct);
                        falhas.Add(ticker + ": HTTP " + (int)response.StatusCode + " - " +
                            corpoErro[..Math.Min(180, corpoErro.Length)]);
                        continue;
                    }
                    await using var stream = await response.Content.ReadAsStreamAsync(ct);
                    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                    if (!doc.RootElement.TryGetProperty("results", out var results) ||
                        results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0 ||
                        !results[0].TryGetProperty("data", out var serie) ||
                        !serie.TryGetProperty("historicalDataPrice", out var pontos) ||
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
                        if (dia < inicio || dia > fim) continue;
                        if (!ponto.TryGetProperty("close", out var fechamento) ||
                            !fechamento.TryGetDecimal(out var preco) || preco <= 0) continue;
                        novos[dia] = preco;
                    }
                    // O ativo foi carregado AsNoTracking: anexá-lo como existente evita INSERT de Ativo.
                    _db.Ativos.Attach(ativo);
                    var existentes = await _db.CotacoesAtivos
                        .Where(c => c.AtivoId == ativo.Id && c.DataReferencia >= inicio &&
                            c.DataReferencia < fim.AddDays(1))
                        .ToListAsync(ct);
                    // Importação histórica nunca elimina snapshots manuais intradiários.
                    // Atualiza somente o registro diário à meia-noite quando já existir.
                    var inseridosAtivo = 0;
                    var atualizadosAtivo = 0;
                    var duplicatasAtivo = 0;
                    var porDataExata = existentes.ToDictionary(c => c.DataReferencia, c => c);
                    foreach (var (dia, preco) in novos)
                    {
                        if (porDataExata.TryGetValue(dia, out var existente))
                        {
                            if (dia != fim && existente.Preco != preco)
                            {
                                existente.Atualizar(existente.DataReferencia, preco);
                                atualizadosAtivo++;
                            }
                        }
                        else if (!existentes.Any(c => c.DataReferencia.Date == dia))
                        {
                            _db.CotacoesAtivos.Add(new CotacaoAtivo(ativo, dia, preco));
                            inseridosAtivo++;
                        }
                        // Se só existem capturas manuais nesse dia, preservá-las.
                    }
                    await _db.SaveChangesAsync(ct);
                    // Só contabiliza gravações após confirmação do banco.
                    inseridos += inseridosAtivo;
                    atualizados += atualizadosAtivo;
                    duplicatasRemovidas += duplicatasAtivo;
                    _db.ChangeTracker.Clear();
                    totalPersistido += await _db.CotacoesAtivos.AsNoTracking()
                        .CountAsync(c => c.AtivoId == ativo.Id && c.DataReferencia >= Inicio && c.DataReferencia < fim.AddDays(1), ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    _db.ChangeTracker.Clear();
                    var detalhe = ex is DbUpdateException dbEx
                        ? dbEx.InnerException?.Message ?? dbEx.Message
                        : ex.Message;
                    falhas.Add(ticker + ": " + ex.GetType().Name + " - " +
                        detalhe[..Math.Min(220, detalhe.Length)]);
                    _logger.LogWarning(ex, "Falha ao importar historico de {Ticker}", ticker);
                }
            }
            return new HistoricoCotacoesResultado(inicio, fim, elegiveis.Count, inseridos, atualizados, duplicatasRemovidas, totalPersistido, falhas);
        }
        finally { Trava.Release(); }
    }
}

public sealed record HistoricoCotacoesResultado(
    DateTime Inicio, DateTime Fim, int AtivosConsultados,
    int Inseridos, int Atualizados, int DuplicatasRemovidas, int TotalPersistido, IReadOnlyList<string> Falhas);
