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
    private readonly MercadoBrapiService _mercadoBrapiService;

    public AssistenteIaService(
        InvestimentosDbContext context,
        ConsultarDashboardHandler dashboardHandler,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        MercadoBrapiService mercadoBrapiService)
    {
        _context = context;
        _dashboardHandler = dashboardHandler;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mercadoBrapiService = mercadoBrapiService;
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

        /*
         * Primeiro lemos a carteira apenas para descobrir quais ativos e
         * opções precisam de cotação. Depois da atualização de mercado,
         * reconstruímos os dashboards para que patrimônio, distância do
         * strike e exposição usem os preços recém-obtidos.
         */
        var tickersAtivos =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
        var opcoesMercado =
            new Dictionary<string, OpcaoMercadoConsulta>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var investidor in investidores)
        {
            var dashboard =
                await _dashboardHandler.HandleAsync(
                    investidor.Id,
                    cancellationToken);

            foreach (var posicao in dashboard.Posicoes
                         .Where(x => x.Quantidade > 0))
            {
                tickersAtivos.Add(posicao.Ticker);
            }

            foreach (var opcao in dashboard.Opcoes
                         .Where(x =>
                             x.Situacao == "ABERTA" ||
                             x.Situacao == "EXECUTADA"))
            {
                tickersAtivos.Add(opcao.TickerAtivo);

                opcoesMercado[opcao.TickerOpcao] =
                    new OpcaoMercadoConsulta(
                        opcao.TickerOpcao,
                        opcao.Vencimento,
                        opcao.Strike);
            }
        }

        var mercado =
            await _mercadoBrapiService.AtualizarAsync(
                tickersAtivos,
                opcoesMercado.Values,
                dataLocal,
                cancellationToken);

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
                        x.Valorizacao,
                        CotacaoAtualizada =
                            mercado.Ativos.TryGetValue(
                                x.Ticker,
                                out var cotacaoAtivo)
                                ? cotacaoAtivo.Preco
                                : (decimal?)null,
                        VariacaoDiaPercentual =
                            mercado.Ativos.TryGetValue(
                                x.Ticker,
                                out var variacaoAtivo)
                                ? variacaoAtivo.VariacaoPercentual
                                : null
                    }),
                Opcoes = dashboard.Opcoes
                    .Where(x =>
                        x.Situacao == "ABERTA" ||
                        x.Situacao == "EXECUTADA")
                    .Select(x =>
                    {
                        mercado.Opcoes.TryGetValue(
                            x.TickerOpcao,
                            out var cotacaoOpcao);

                        decimal? custoRecompra = null;
                        decimal? ganhoRecompra = null;
                        decimal? percentualPremioCapturado = null;

                        if (cotacaoOpcao is not null &&
                            x.Natureza == "VENDA")
                        {
                            custoRecompra =
                                cotacaoOpcao.Preco *
                                x.Quantidade;

                            ganhoRecompra =
                                x.PremioTotal -
                                custoRecompra.Value -
                                x.Taxas;

                            if (x.PremioTotal > 0)
                            {
                                percentualPremioCapturado =
                                    ganhoRecompra.Value /
                                    x.PremioTotal *
                                    100m;
                            }
                        }

                        return new
                        {
                            x.TickerAtivo,
                            x.TickerOpcao,
                            x.TipoOpcao,
                            x.Natureza,
                            x.Strike,
                            x.Quantidade,
                            x.PremioUnitario,
                            x.PremioTotal,
                            x.Taxas,
                            x.Vencimento,
                            x.Situacao,
                            x.ValorExecucao,
                            ExercicioOuAtribuicaoConfirmado =
                                x.ValorExecucao.HasValue &&
                                x.ValorExecucao.Value > 0,
                            x.ValorAcaoAtual,
                            x.DistanciaStrikePercentual,
                            x.EmRiscoExercicio,
                            CotacaoOpcao =
                                cotacaoOpcao?.Preco,
                            DataCotacaoOpcao =
                                cotacaoOpcao?.DataReferencia,
                            CustoRecompraEstimado =
                                custoRecompra,
                            GanhoRecompraEstimado =
                                ganhoRecompra,
                            PercentualPremioCapturado =
                                percentualPremioCapturado
                        };
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

        await EnviarWhatsAppAsync(
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


    private async Task EnviarWhatsAppAsync(
        RelatorioDiarioIa relatorio,
        CancellationToken cancellationToken)
    {
        var token =
            _configuration["WHATSAPP_ACCESS_TOKEN"];
        var phoneNumberId =
            _configuration["WHATSAPP_PHONE_NUMBER_ID"];
        var destinatario =
            _configuration["WHATSAPP_TO"];
        var template =
            _configuration["WHATSAPP_TEMPLATE_NAME"];
        var idioma =
            _configuration["WHATSAPP_TEMPLATE_LANGUAGE"]
            ?? "pt_BR";
        var graphVersion =
            _configuration["WHATSAPP_GRAPH_VERSION"]
            ?? "v24.0";

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(phoneNumberId) ||
            string.IsNullOrWhiteSpace(destinatario) ||
            string.IsNullOrWhiteSpace(template))
            return;

        var resumo = relatorio.Conteudo;
        if (resumo.Length > 3000)
            resumo = resumo[..3000];

        var payload =
            JsonSerializer.Serialize(new
            {
                messaging_product = "whatsapp",
                to = destinatario,
                type = "template",
                template = new
                {
                    name = template,
                    language = new
                    {
                        code = idioma
                    },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new object[]
                            {
                                new
                                {
                                    type = "text",
                                    text =
                                        relatorio.DataReferencia
                                            .ToString("dd/MM/yyyy")
                                },
                                new
                                {
                                    type = "text",
                                    text = resumo
                                }
                            }
                        }
                    }
                }
            });

        var client =
            _httpClientFactory.CreateClient();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/{graphVersion}/{phoneNumberId}/messages");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content =
            new StringContent(
                payload,
                Encoding.UTF8,
                "application/json");

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var erro =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"WhatsApp retornou {(int)response.StatusCode}: {erro}");
        }
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
