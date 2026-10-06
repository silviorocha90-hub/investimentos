using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Investimentos.Application.Dashboard;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.AssistenteIa;

public sealed class AssistenteIaService
{
    private readonly InvestimentosDbContext _context;
    private readonly ConsultarDashboardHandler _dashboardHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AssistenteIaService(
        InvestimentosDbContext context,
        ConsultarDashboardHandler dashboardHandler,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _context = context;
        _dashboardHandler = dashboardHandler;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<RelatorioDiarioIa>> ListarAsync(
        CancellationToken cancellationToken)
    {
        return await _context.RelatoriosDiariosIa
            .AsNoTracking()
            .OrderByDescending(x => x.DataReferencia)
            .Take(90)
            .ToListAsync(cancellationToken);
    }

    public async Task<RelatorioDiarioIa?> ObterHojeAsync(
        DateTime dataLocal,
        CancellationToken cancellationToken)
    {
        var data = dataLocal.Date;

        return await _context.RelatoriosDiariosIa
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.DataReferencia == data,
                cancellationToken);
    }

    public async Task<RelatorioDiarioIa> GerarAsync(
        DateTime dataLocal,
        bool substituir,
        CancellationToken cancellationToken)
    {
        var data = dataLocal.Date;
        var existente =
            await _context.RelatoriosDiariosIa
                .FirstOrDefaultAsync(
                    x => x.DataReferencia == data,
                    cancellationToken);

        if (existente is not null && !substituir)
            return existente;

        var apiKey =
            _configuration["OPENAI_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "OPENAI_API_KEY não configurada.");

        var modelo =
            _configuration["OPENAI_MODEL"]
            ?? "gpt-6-luna";

        var investidores =
            await _context.Investidores
                .AsNoTracking()
                .OrderBy(x => x.Nome)
                .Select(x => new { x.Id, x.Nome })
                .ToListAsync(cancellationToken);

        var carteiras = new List<object>();

        foreach (var investidor in investidores)
        {
            var dashboard =
                await _dashboardHandler.HandleAsync(
                    investidor.Id,
                    cancellationToken);

            carteiras.Add(new
            {
                investidor.Nome,
                dashboard.PatrimonioEstimado,
                dashboard.CaixaDisponivel,
                Posicoes = dashboard.Posicoes
                    .Where(x => x.Quantidade > 0)
                    .Select(x => new
                    {
                        x.Ticker,
                        x.Quantidade,
                        x.PrecoMedio,
                        x.ValorAtual,
                        x.Valorizacao
                    }),
                Opcoes = dashboard.Opcoes
                    .Where(x =>
                        x.Situacao == "ABERTA" ||
                        x.Situacao == "EXECUTADA")
                    .Select(x => new
                    {
                        x.TickerAtivo,
                        x.TickerOpcao,
                        x.TipoOpcao,
                        x.Natureza,
                        x.Strike,
                        x.Quantidade,
                        x.PremioTotal,
                        x.Vencimento,
                        x.Situacao,
                        x.ValorAcaoAtual,
                        x.DistanciaStrikePercentual,
                        x.EmRiscoExercicio
                    })
            });
        }

        var dados =
            JsonSerializer.Serialize(
                carteiras,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        var instructions = """
Você é o Assistente IA de uma carteira de investimentos brasileira.
Analise somente ativos e opções presentes nos dados fornecidos.
Use pesquisa na web para buscar informações atuais e relevantes do dia,
priorizando fontes oficiais (RI das empresas, CVM, B3) e veículos financeiros
confiáveis. Não invente preços, fatos relevantes, dividendos ou datas.
Diferencie fatos confirmados de interpretação. Não dê ordem automática de
compra ou venda. Destaque: fatos relevantes, resultados, dividendos/JCP,
eventos corporativos, movimentos materiais, riscos e proximidade de strikes.
Quando não houver novidade relevante, diga explicitamente.
Produza texto em português do Brasil, conciso, organizado em:
1. Resumo executivo;
2. Alertas prioritários;
3. Ativos com novidades;
4. Opções e strikes;
5. Proventos/eventos;
6. O que acompanhar no próximo pregão.
Sempre relacione a notícia à posição concreta informada.
""";

        var input =
            $"Data de referência: {data:dd/MM/yyyy}.\n" +
            "Dados atuais da carteira:\n" +
            dados;

        var payload =
            JsonSerializer.Serialize(new
            {
                model = modelo,
                instructions,
                input,
                tools = new[]
                {
                    new { type = "web_search" }
                }
            });

        var client =
            _httpClientFactory.CreateClient();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        request.Content =
            new StringContent(
                payload,
                Encoding.UTF8,
                "application/json");

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"OpenAI retornou {(int)response.StatusCode}: {json}");

        var conteudo =
            ExtrairTexto(json);

        if (string.IsNullOrWhiteSpace(conteudo))
            throw new InvalidOperationException(
                "A IA não retornou conteúdo textual.");

        if (existente is not null)
            _context.RelatoriosDiariosIa.Remove(existente);

        var relatorio =
            new RelatorioDiarioIa(
                data,
                conteudo,
                modelo);

        _context.RelatoriosDiariosIa.Add(relatorio);
        await _context.SaveChangesAsync(cancellationToken);

        await EnviarTelegramAsync(
            relatorio,
            cancellationToken);

        return relatorio;
    }

    private static string ExtrairTexto(string json)
    {
        using var document =
            JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty(
                "output",
                out var output))
            return string.Empty;

        var textos = new List<string>();

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty(
                    "content",
                    out var content))
                continue;

            foreach (var parte in content.EnumerateArray())
            {
                if (parte.TryGetProperty(
                        "type",
                        out var type) &&
                    type.GetString() == "output_text" &&
                    parte.TryGetProperty(
                        "text",
                        out var text))
                {
                    textos.Add(
                        text.GetString()
                        ?? string.Empty);
                }
            }
        }

        return string.Join(
            Environment.NewLine,
            textos);
    }

    private async Task EnviarTelegramAsync(
        RelatorioDiarioIa relatorio,
        CancellationToken cancellationToken)
    {
        var token =
            _configuration["TELEGRAM_BOT_TOKEN"];
        var chatId =
            _configuration["TELEGRAM_CHAT_ID"];

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(chatId))
            return;

        var texto =
            $"Relatório da carteira — {relatorio.DataReferencia:dd/MM/yyyy}\n\n" +
            relatorio.Conteudo;

        if (texto.Length > 4000)
            texto = texto[..4000];

        var client =
            _httpClientFactory.CreateClient();

        using var response =
            await client.PostAsync(
                $"https://api.telegram.org/bot{token}/sendMessage",
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["chat_id"] = chatId,
                        ["text"] = texto
                    }),
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
