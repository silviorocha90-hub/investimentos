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
    private readonly Investimentos.Api.ExceptionHandling.LogAplicacaoService _logs;

    public MercadoBrapiService(
        InvestimentosDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MercadoBrapiService> logger,
        Investimentos.Api.ExceptionHandling.LogAplicacaoService logs)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _logs = logs;
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
            // Instrumentos sem ticker de mercado não devem chegar à Brapi.
            .Where(x => !x.Contains("FMP", StringComparison.OrdinalIgnoreCase) &&
                        !x.Contains("ELETROBRAS", StringComparison.OrdinalIgnoreCase) &&
                        !x.Contains("PREVIDENCIA", StringComparison.OrdinalIgnoreCase) &&
                        !x.Contains(" ", StringComparison.Ordinal))
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

        // A brapi exige plano Pro para opções fora do sandbox PETR.
        // Não consultar endpoints pagos sem ativação explícita do operador.
        var consultarOpcoes = bool.TryParse(
            _configuration["BRAPI_OPCOES_HABILITADAS"], out var habilitadas) &&
            habilitadas;

        var opcoesDistintas = opcoes
            .Where(x => !string.IsNullOrWhiteSpace(x.TickerOpcao))
            .GroupBy(x => x.TickerOpcao, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();

        foreach (var opcao in opcoesDistintas)
        {
            CotacaoOpcaoMercado? cotacao = null;
            if (consultarOpcoes)
                cotacao = await ObterCotacaoOpcaoAsync(opcao, cancellationToken);

            if (cotacao is not null)
                cotacoesOpcoes[opcao.TickerOpcao] = cotacao;
        }

        // Google Finance e uma fonte indicativa e opcional: somente apos falha da Brapi.
        // A pagina publica nao oferece API de cotacoes garantida para opcoes B3.
        var consultarGoogle = bool.TryParse(
            _configuration["GOOGLE_FINANCE_OPCOES_HABILITADAS"], out var googleHabilitado) &&
            googleHabilitado;
        if (consultarGoogle)
        {
            foreach (var opcao in opcoesDistintas.Where(x =>
                         !cotacoesOpcoes.ContainsKey(x.TickerOpcao)))
            {
                var cotacao = await ObterCotacaoGoogleFinanceAsync(opcao, cancellationToken);
                if (cotacao is not null)
                    cotacoesOpcoes[opcao.TickerOpcao] = cotacao;
            }
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

        // O plano gratuito autoriza apenas um ticker por requisição.
        // Tickers já foram deduplicados no método chamador.
        var client = CriarCliente();
        foreach (var ticker in tickers)
        {
            var url = "https://brapi.dev/api/v2/stocks/quote?symbols=" +
                Uri.EscapeDataString(ticker);
            try
            {
                using var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "brapi não retornou preço para {Ticker}. HTTP {Status}.",
                        ticker, (int)response.StatusCode);
                    await _logs.RegistrarAsync("Cotacoes", "Consultar " + ticker,
                        new HttpRequestException("Brapi retornou HTTP " + (int)response.StatusCode),
                        ticker, cancellationToken);
                    continue;
                }

                await using var stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(
                    stream, cancellationToken: cancellationToken);
                if (!document.RootElement.TryGetProperty("results", out var items))
                    continue;

                foreach (var item in items.EnumerateArray())
                {
                    var symbol = ObterString(item, "symbol") ??
                        ObterString(item, "requestedSymbol");
                    JsonElement data = item;
                    if (item.TryGetProperty("data", out var nested) &&
                        nested.ValueKind == JsonValueKind.Object)
                        data = nested;

                    var preco = ObterDecimal(data, "regularMarketPrice");
                    if (string.IsNullOrWhiteSpace(symbol) ||
                        !preco.HasValue || preco.Value <= 0)
                        continue;

                    resultado[symbol] = new CotacaoAtivoMercado(
                        symbol, preco.Value,
                        ObterDecimal(data, "regularMarketChangePercent"),
                        DateTimeOffset.UtcNow);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Falha ao consultar cotação de {Ticker} na brapi.", ticker);
                await _logs.RegistrarAsync("Cotacoes", "Consultar " + ticker, ex,
                    ticker, cancellationToken);
            }
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

        // Preserve the time of the successful quote refresh, not just its calendar day.
        var data = dataLocal;

        foreach (var item in cotacoes.Values)
        {
            if (!porTicker.TryGetValue(item.Ticker, out var ativo))
                continue;

            var existente = await _context.CotacoesAtivos
                .FirstOrDefaultAsync(
                    x => x.AtivoId == ativo.Id &&
                         x.DataReferencia.Date == data.Date,
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

    // Consulta indicativa da pagina publica Google Finance, sem pesquisa generica
    // ou scraping de resultados do buscador. Nao grava valores sem prova de ticker.
    private async Task<CotacaoOpcaoMercado?> ObterCotacaoGoogleFinanceAsync(
        OpcaoMercadoConsulta opcao, CancellationToken cancellationToken)
    {
        var ticker = opcao.TickerOpcao.Trim().ToUpperInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(ticker, @"^[A-Z0-9]{5,15}$"))
            return null;

        var url = "https://www.google.com/finance/quote/" +
            Uri.EscapeDataString(ticker) + ":BVMF";
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Google Finance: {Ticker} HTTP {Status}.",
                    ticker, (int)response.StatusCode);
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            // Exige identidade exata do instrumento no titulo da pagina.
            // Se a pagina nao possuir dados reconheciveis, nao inventar preco.
            var title = System.Text.RegularExpressions.Regex.Match(html,
                @"<title[^>]*>(?<title>[^<]*)</title>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!title.Success ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                    System.Net.WebUtility.HtmlDecode(title.Groups["title"].Value),
                    @"\b" + System.Text.RegularExpressions.Regex.Escape(ticker) +
                    @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return null;

            // O Google Finance usa o elemento YMlKec para o preco principal.
            // Validar moeda BRL para evitar cotacoes de instrumentos homonimos.
            if (!html.Contains("BRL", StringComparison.OrdinalIgnoreCase) &&
                !html.Contains("R$", StringComparison.Ordinal))
                return null;
            var match = System.Text.RegularExpressions.Regex.Match(html,
                @"<div[^>]*class=""[^""]*YMlKec[^""]*""[^>]*>\s*(?:R\$\s*)?(?<preco>[0-9][0-9.,]*)\s*</div>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var texto = match.Groups["preco"].Value;
            if (texto.Contains(','))
                texto = texto.Replace(".", "").Replace(',', '.');
            if (!decimal.TryParse(texto, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var preco) || preco <= 0)
                return null;

            // Data/hora nao e garantida: manter DataReferencia nula.
            _logger.LogInformation(
                "Google Finance: preco indicativo encontrado para {Ticker}; horario desconhecido.",
                ticker);
            return new CotacaoOpcaoMercado(ticker, preco, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Google Finance: consulta indisponivel para {Ticker} ({Tipo}).",
                ticker, ex.GetType().Name);
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
