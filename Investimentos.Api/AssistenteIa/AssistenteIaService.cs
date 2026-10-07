using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Investimentos.Application.Dashboard;
using Investimentos.Domain.Entities;
using Investimentos.Domain.Usuarios;
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
    private readonly ILogger<AssistenteIaService> _logger;

    public AssistenteIaService(
        InvestimentosDbContext context,
        ConsultarDashboardHandler dashboardHandler,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        MercadoBrapiService mercadoBrapiService,
        ILogger<AssistenteIaService> logger)
    {
        _context = context;
        _dashboardHandler = dashboardHandler;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mercadoBrapiService = mercadoBrapiService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RelatorioDiarioIa>> ListarAsync(
        Guid usuarioId,
        bool administrador,
        CancellationToken cancellationToken)
    {
        if (administrador)
        {
            return await _context.RelatoriosDiariosIa
                .AsNoTracking()
                .Where(x => x.Escopo == "TODOS")
                .OrderByDescending(x => x.DataReferencia)
                .Take(90)
                .ToListAsync(cancellationToken);
        }

        var investidoresIds = await _context.Usuarios
            .AsNoTracking()
            .Where(x => x.Id == usuarioId)
            .SelectMany(x => x.Investidores)
            .Select(x => x.InvestidorId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        return await _context.RelatoriosDiariosIa
            .AsNoTracking()
            .Where(x =>
                x.Escopo == "INVESTIDOR" &&
                x.InvestidorId.HasValue &&
                investidoresIds.Contains(x.InvestidorId.Value))
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
                x => x.DataReferencia == data &&
                     x.Escopo == "TODOS",
                cancellationToken);
    }

    public Task<RelatorioDiarioIa> GerarAsync(
        DateTime dataLocal,
        bool substituir,
        CancellationToken cancellationToken)
    {
        return GerarRelatorioAsync(
            dataLocal,
            substituir,
            null,
            "TODOS",
            cancellationToken);
    }

    public async Task ProcessarEnviosAsync(
        DateTime dataLocal,
        CancellationToken cancellationToken)
    {
        var relatorioAdmin = await ObterHojeAsync(dataLocal, cancellationToken)
            ?? await GerarAsync(dataLocal, false, cancellationToken);

        var administradores = await _context.Usuarios
            .Where(x =>
                x.Perfil == PerfilUsuario.Admin &&
                x.Status == StatusUsuario.Ativo &&
                x.ReceberRelatorioIa)
            .ToListAsync(cancellationToken);

        foreach (var administrador in administradores)
        {
            if (!DeveEnviar(
                    dataLocal.Date,
                    administrador.UltimoEnvioRelatorioIa,
                    administrador.FrequenciaRelatorioIa))
                continue;

            try
            {
                await EnviarRelatorioAsync(
                    relatorioAdmin,
                    administrador,
                    cancellationToken);

                administrador.RegistrarEnvioRelatorioIa(dataLocal);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao enviar relatório IA consolidado ao administrador {AdministradorId}.",
                    administrador.Id);
            }
        }

        var usuarios = await _context.Usuarios
            .Where(x =>
                x.Perfil == PerfilUsuario.Usuario &&
                x.Status == StatusUsuario.Ativo &&
                x.ReceberRelatorioIa)
            .Include(x => x.Investidores)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        foreach (var usuario in usuarios)
        {
            if (!DeveEnviar(
                    dataLocal.Date,
                    usuario.UltimoEnvioRelatorioIa,
                    usuario.FrequenciaRelatorioIa))
                continue;

            var investidoresIds = usuario.Investidores
                .Select(x => x.InvestidorId)
                .Distinct()
                .ToArray();

            if (investidoresIds.Length == 0)
                continue;

            var todosEnviados = true;

            foreach (var investidorId in investidoresIds)
            {
                try
                {
                    var relatorio = await GerarRelatorioAsync(
                        dataLocal,
                        false,
                        investidorId,
                        "INVESTIDOR",
                        cancellationToken);

                    await EnviarRelatorioAsync(
                        relatorio,
                        usuario,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    todosEnviados = false;
                    _logger.LogError(
                        ex,
                        "Falha ao gerar/enviar relatório IA do investidor {InvestidorId} para o usuário {UsuarioId}.",
                        investidorId,
                        usuario.Id);
                }
            }

            if (todosEnviados)
            {
                usuario.RegistrarEnvioRelatorioIa(dataLocal);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task<int> ReenviarRelatorioAsync(
        Guid usuarioId,
        DateTime dataLocal,
        CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(x => x.Investidores)
            .FirstOrDefaultAsync(
                x => x.Id == usuarioId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Usuário não encontrado.");

        if (usuario.Status != StatusUsuario.Ativo)
            throw new InvalidOperationException(
                "O usuário precisa estar ativo para receber o relatório.");

        if (!usuario.ReceberRelatorioIa)
            throw new InvalidOperationException(
                "O recebimento do relatório IA está desativado para este usuário.");

        var data = dataLocal.Date;
        var relatorios = new List<RelatorioDiarioIa>();

        if (usuario.Perfil == PerfilUsuario.Admin)
        {
            var relatorio = await _context.RelatoriosDiariosIa
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.DataReferencia == data &&
                         x.Escopo == "TODOS",
                    cancellationToken);

            if (relatorio is not null)
                relatorios.Add(relatorio);
        }
        else
        {
            var investidoresIds = usuario.Investidores
                .Select(x => x.InvestidorId)
                .Distinct()
                .ToArray();

            if (investidoresIds.Length == 0)
                throw new InvalidOperationException(
                    "O usuário não possui investidores vinculados.");

            relatorios = await _context.RelatoriosDiariosIa
                .AsNoTracking()
                .Where(x =>
                    x.DataReferencia == data &&
                    x.Escopo == "INVESTIDOR" &&
                    x.InvestidorId.HasValue &&
                    investidoresIds.Contains(x.InvestidorId.Value))
                .OrderBy(x => x.InvestidorId)
                .ToListAsync(cancellationToken);
        }

        if (relatorios.Count == 0)
            throw new InvalidOperationException(
                $"Não existe relatório salvo para {data:dd/MM/yyyy}. O reenvio não gera um novo relatório.");

        foreach (var relatorio in relatorios)
        {
            await EnviarRelatorioAsync(
                relatorio,
                usuario,
                cancellationToken);
        }

        usuario.RegistrarEnvioRelatorioIa(dataLocal);
        await _context.SaveChangesAsync(cancellationToken);

        return relatorios.Count;
    }

    private static bool DeveEnviar(
        DateTime hoje,
        DateTime? ultimoEnvio,
        string frequencia)
    {
        if (!ultimoEnvio.HasValue)
            return true;

        var ultimo = ultimoEnvio.Value.Date;

        return frequencia switch
        {
            "SEMANAL" => hoje >= ultimo.AddDays(7),
            "QUINZENAL" => hoje >= ultimo.AddDays(15),
            "MENSAL" => hoje >= ultimo.AddMonths(1),
            _ => hoje > ultimo
        };
    }

    private async Task<RelatorioDiarioIa> GerarRelatorioAsync(
        DateTime dataLocal,
        bool substituir,
        Guid? investidorId,
        string escopo,
        CancellationToken cancellationToken)
    {
        var data = dataLocal.Date;
        var existente = await _context.RelatoriosDiariosIa
            .FirstOrDefaultAsync(
                x => x.DataReferencia == data &&
                     x.Escopo == escopo &&
                     x.InvestidorId == investidorId,
                cancellationToken);

        if (existente is not null && !substituir)
            return existente;

        var apiKey = _configuration["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "OPENAI_API_KEY não configurada.");

        var modelo = _configuration["OPENAI_MODEL"]
            ?? "gpt-6-luna";

        var investidoresQuery = _context.Investidores
            .AsNoTracking()
            .AsQueryable();

        if (investidorId.HasValue)
            investidoresQuery = investidoresQuery
                .Where(x => x.Id == investidorId.Value);

        var investidores = await investidoresQuery
            .OrderBy(x => x.Nome)
            .Select(x => new { x.Id, x.Nome })
            .ToListAsync(cancellationToken);

        if (investidores.Count == 0)
            throw new InvalidOperationException(
                "Nenhum investidor encontrado para o relatório.");

        var tickersAtivos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var opcoesMercado =
            new Dictionary<string, OpcaoMercadoConsulta>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var investidor in investidores)
        {
            var dashboard = await _dashboardHandler.HandleAsync(
                investidor.Id,
                cancellationToken);

            foreach (var posicao in dashboard.Posicoes
                         .Where(x => x.Quantidade > 0))
                tickersAtivos.Add(posicao.Ticker);

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

        var mercado = await _mercadoBrapiService.AtualizarAsync(
            tickersAtivos,
            opcoesMercado.Values,
            dataLocal,
            cancellationToken);

        var carteiras = new List<object>();

        foreach (var investidor in investidores)
        {
            var dashboard = await _dashboardHandler.HandleAsync(
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
                                cotacaoOpcao.Preco * x.Quantidade;
                            ganhoRecompra =
                                x.PremioTotal -
                                custoRecompra.Value -
                                x.Taxas;

                            if (x.PremioTotal > 0)
                                percentualPremioCapturado =
                                    ganhoRecompra.Value /
                                    x.PremioTotal * 100m;
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
                            CotacaoOpcao = cotacaoOpcao?.Preco,
                            DataCotacaoOpcao =
                                cotacaoOpcao?.DataReferencia,
                            CustoRecompraEstimado = custoRecompra,
                            GanhoRecompraEstimado = ganhoRecompra,
                            PercentualPremioCapturado =
                                percentualPremioCapturado
                        };
                    })
            });
        }

        var dados = JsonSerializer.Serialize(
            carteiras,
            new JsonSerializerOptions { WriteIndented = true });

        var instructions = escopo == "TODOS"
            ? InstrucoesBase +
              "\nEste é o relatório administrativo consolidado. Analise TODAS as carteiras fornecidas e destaque também concentrações e riscos consolidados."
            : InstrucoesBase +
              "\nEste relatório pertence a um único investidor. Analise EXCLUSIVAMENTE a carteira fornecida. Não mencione, compare nem revele dados de outros investidores.";

        var input =
            $"Data de referência: {data:dd/MM/yyyy}.\n" +
            $"Escopo: {escopo}.\n" +
            "Dados atuais da carteira:\n" +
            dados;

        var payload = JsonSerializer.Serialize(new
        {
            model = modelo,
            instructions,
            input,
            tools = new[] { new { type = "web_search" } }
        });

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/responses");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(
            request,
            cancellationToken);
        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"OpenAI retornou {(int)response.StatusCode}: {json}");

        var conteudo = ExtrairTexto(json);
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new InvalidOperationException(
                "A IA não retornou conteúdo textual.");

        if (existente is not null)
            _context.RelatoriosDiariosIa.Remove(existente);

        var relatorio = new RelatorioDiarioIa(
            data,
            conteudo,
            modelo,
            escopo,
            investidorId);

        _context.RelatoriosDiariosIa.Add(relatorio);
        await _context.SaveChangesAsync(cancellationToken);

        return relatorio;
    }

    private const string InstrucoesBase = """
Você é o Assistente IA de uma carteira de investimentos brasileira.
Analise somente ativos e opções presentes nos dados fornecidos.
Use pesquisa na web para buscar informações atuais e relevantes do dia,
priorizando fontes oficiais (RI das empresas, CVM, B3) e veículos financeiros
confiáveis. Não invente preços, fatos relevantes, dividendos ou datas.
Diferencie fatos confirmados de interpretação. Não dê ordem automática de
compra ou venda. Destaque fatos relevantes, resultados, dividendos/JCP,
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

    private static string ExtrairTexto(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty(
                "output",
                out var output))
            return string.Empty;

        var textos = new List<string>();
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content))
                continue;

            foreach (var parte in content.EnumerateArray())
            {
                if (parte.TryGetProperty("type", out var type) &&
                    type.GetString() == "output_text" &&
                    parte.TryGetProperty("text", out var text))
                    textos.Add(text.GetString() ?? string.Empty);
            }
        }

        return string.Join(Environment.NewLine, textos);
    }

    private async Task EnviarRelatorioAsync(
        RelatorioDiarioIa relatorio,
        Usuario usuario,
        CancellationToken cancellationToken)
    {
        await EnviarEmailAsync(
            relatorio,
            usuario.Email,
            usuario.Nome,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(usuario.TelegramChatId))
        {
            await EnviarTelegramAsync(
                relatorio,
                usuario.TelegramChatId,
                cancellationToken);
        }
    }

    private async Task EnviarEmailAsync(
        RelatorioDiarioIa relatorio,
        string destinatario,
        string nome,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["RESEND_API_KEY"];
        var remetente = _configuration["EMAIL_FROM"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "RESEND_API_KEY não configurada.");

        if (string.IsNullOrWhiteSpace(remetente))
            throw new InvalidOperationException(
                "EMAIL_FROM não configurado.");

        var assunto =
            $"Relatório de investimentos — {relatorio.DataReferencia:dd/MM/yyyy}";
        var texto =
            $"Olá, {nome}.\n\n" +
            relatorio.Conteudo +
            "\n\nRelatório gerado automaticamente pelo Assistente IA.";

        var payload = JsonSerializer.Serialize(new
        {
            from = remetente,
            to = new[] { destinatario },
            subject = assunto,
            text = texto
        });

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.resend.com/emails");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync(
                cancellationToken);
            throw new InvalidOperationException(
                $"Resend retornou {(int)response.StatusCode}: {erro}");
        }
    }

    private async Task EnviarTelegramAsync(
        RelatorioDiarioIa relatorio,
        string chatId,
        CancellationToken cancellationToken)
    {
        var token = _configuration["TELEGRAM_BOT_TOKEN"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "TELEGRAM_BOT_TOKEN não configurado.");

        var cabecalho =
            $"Relatório de investimentos — {relatorio.DataReferencia:dd/MM/yyyy}\n\n";
        var conteudo = relatorio.Conteudo;
        const int limite = 3900;

        var partes = new List<string>();
        while (conteudo.Length > limite)
        {
            var corte = conteudo.LastIndexOf(
                '\n',
                limite);

            if (corte < limite / 2)
                corte = limite;

            partes.Add(conteudo[..corte]);
            conteudo = conteudo[corte..].TrimStart();
        }

        if (conteudo.Length > 0)
            partes.Add(conteudo);

        var client = _httpClientFactory.CreateClient();

        for (var i = 0; i < partes.Count; i++)
        {
            var texto = (i == 0 ? cabecalho : string.Empty) +
                partes[i];

            using var response = await client.PostAsync(
                $"https://api.telegram.org/bot{token}/sendMessage",
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["chat_id"] = chatId,
                        ["text"] = texto
                    }),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync(
                    cancellationToken);
                throw new InvalidOperationException(
                    $"Telegram retornou {(int)response.StatusCode}: {erro}");
            }
        }
    }
}
