using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.AssistenteIa;

public sealed class MercadoBrapiService
{
    private readonly InvestimentosDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MercadoBrapiService> _logger;

    public MercadoBrapiService(
        InvestimentosDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MercadoBrapiService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AtualizacaoMercadoResultado> AtualizarAsync(
        IEnumerable<string> tickersAtivos,
        IEnumerable<OpcaoMercadoConsulta> opcoes,
        DateTime dataLocal,
        CancellationToken cancellationToken)
    {
        var tickers = tickersAtivos
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var cotacoesAtivos = await ObterCotacoesAtivosAsync(
            tickers,
            cancellationToken);

        await PersistirCotacoesAtivosAsync(
            cotacoesAtivos,
            dataLocal,
            cancellationToken);

        var cotacoesOpcoes =
            new Dictionary<string, CotacaoOpcaoMercado>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var opcao in opcoes
                     .Where(x => !string.IsNullOrWhiteSpace(x.TickerOpcao))
                     .GroupBy(x => x.TickerOpcao, StringComparer.OrdinalIgnoreCase)
                     .Select(x => x.First()))
        {
            var cotacao = await ObterCotacaoOpcaoAsync(
                opcao,
                cancellationToken);

            if (cotacao is not null)
                cotacoesOpcoes[opcao.TickerOpcao] = cotacao;
        }

        return new AtualizacaoMercadoResultado(
            cotacoesAtivos,
            cotacoesOpcoes);
    }

    private async Task<Dictionary<string, CotacaoAtivoMercado>>
        ObterCotacoesAtivosAsync(
            IReadOnlyCollection<string> tickers,
            CancellationToken cancellationToken)
    {
        var resultado =
            new Dictionary<string, CotacaoAtivoMercado>(
                StringComparer.OrdinalIgnoreCase);

        if (tickers.Count == 0)
            return resultado;

        var client = CriarCliente();
        var url =
            "https://brapi.dev/api/v2/stocks/quote?symbols=" +
            Uri.EscapeDataString(string.Join(",", tickers));

        try
        {
            using var response =
                await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "brapi não atualizou ativos. HTTP {Status}.",
                    (int)response.StatusCode);
                return resultado;
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document =
                await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("results", out var items))
                return resultado;

            foreach (var item in items.EnumerateArray())
            {
                var symbol =
                    ObterString(item, "symbol") ??
                    ObterString(item, "requestedSymbol");

                JsonElement data = item;
                if (item.TryGetProperty("data", out var dataElement) &&
                    dataElement.ValueKind == JsonValueKind.Object)
                    data = dataElement;

                var preco = ObterDecimal(data, "regularMarketPrice");
                if (string.IsNullOrWhiteSpace(symbol) ||
                    !preco.HasValue ||
                    preco.Value <= 0)
                    continue;

                resultado[symbol] =
                    new CotacaoAtivoMercado(
                        symbol,
                        preco.Value,
                        ObterDecimal(data, "regularMarketChangePercent"),
                        DateTimeOffset.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao consultar cotações de ativos na brapi.");
        }

        return resultado;
    }

    private async Task PersistirCotacoesAtivosAsync(
        IReadOnlyDictionary<string, CotacaoAtivoMercado> cotacoes,
        DateTime dataLocal,
        CancellationToken cancellationToken)
    {
        if (cotacoes.Count == 0)
            return;

        var ativos = await _context.Ativos.ToListAsync(cancellationToken);
        var porTicker = ativos
            .Where(x => cotacoes.ContainsKey(x.Ticker.Codigo))
            .ToDictionary(
                x => x.Ticker.Codigo,
                StringComparer.OrdinalIgnoreCase);

        var data = dataLocal.Date;

        foreach (var item in cotacoes.Values)
        {
            if (!porTicker.TryGetValue(item.Ticker, out var ativo))
                continue;

            var existente = await _context.CotacoesAtivos
                .FirstOrDefaultAsync(
                    x => x.AtivoId == ativo.Id &&
                         x.DataReferencia == data,
                    cancellationToken);

            if (existente is null)
            {
                _context.CotacoesAtivos.Add(
                    new CotacaoAtivo(
                        ativo,
                        data,
                        item.Preco));
            }
            else
            {
                existente.Atualizar(
                    data,
                    item.Preco);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<CotacaoOpcaoMercado?> ObterCotacaoOpcaoAsync(
        OpcaoMercadoConsulta opcao,
        CancellationToken cancellationToken)
    {
        var client = CriarCliente();

        var url =
            "https://brapi.dev/api/v2/options/historical" +
            $"?symbol={Uri.EscapeDataString(opcao.TickerOpcao)}" +
            $"&expirationDate={opcao.Vencimento:yyyy-MM-dd}" +
            $"&strike={opcao.Strike.ToString(CultureInfo.InvariantCulture)}" +
            "&sortOrder=desc";

        try
        {
            using var response =
                await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream =
                await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document =
                await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("option", out var option) ||
                !option.TryGetProperty("history", out var history) ||
                history.ValueKind != JsonValueKind.Array)
                return null;

            var ponto = history.EnumerateArray().FirstOrDefault();
            if (ponto.ValueKind != JsonValueKind.Object)
                return null;

            var preco =
                ObterDecimal(ponto, "close") ??
                ObterDecimal(ponto, "referencePrice") ??
                ObterDecimal(ponto, "average");

            if (!preco.HasValue || preco.Value < 0)
                return null;

            DateTimeOffset? data = null;
            if (ponto.TryGetProperty("date", out var dateElement) &&
                dateElement.TryGetInt64(out var unix))
                data = DateTimeOffset.FromUnixTimeSeconds(unix);

            return new CotacaoOpcaoMercado(
                opcao.TickerOpcao,
                preco.Value,
                data);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                ex,
                "Cotação da opção {TickerOpcao} indisponível.",
                opcao.TickerOpcao);
            return null;
        }
    }

    private HttpClient CriarCliente()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var token = _configuration["BRAPI_TOKEN"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private static string? ObterString(
        JsonElement element,
        string property)
    {
        return element.TryGetProperty(property, out var value) &&
               value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static decimal? ObterDecimal(
        JsonElement element,
        string property)
    {
        if (!element.TryGetProperty(property, out var value) ||
            value.ValueKind == JsonValueKind.Null)
            return null;

        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetDecimal(out var numero))
            return numero;

        if (value.ValueKind == JsonValueKind.String &&
            decimal.TryParse(
                value.GetString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out numero))
            return numero;

        return null;
    }
}

public sealed record OpcaoMercadoConsulta(
    string TickerOpcao,
    DateTime Vencimento,
    decimal Strike);

public sealed record CotacaoAtivoMercado(
    string Ticker,
    decimal Preco,
    decimal? VariacaoPercentual,
    DateTimeOffset DataConsulta);

public sealed record CotacaoOpcaoMercado(
    string TickerOpcao,
    decimal Preco,
    DateTimeOffset? DataReferencia);

public sealed record AtualizacaoMercadoResultado(
    IReadOnlyDictionary<string, CotacaoAtivoMercado> Ativos,
    IReadOnlyDictionary<string, CotacaoOpcaoMercado> Opcoes);
