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

    // Agente de atualização semanal: consulta ativos e opções uma única vez
    // antes da análise, sem utilizar a API da OpenAI.
    public async Task<AtualizacaoMercadoResultado> AtualizarMercadoSemanalAsync(
        DateTime dataLocal,
        CancellationToken cancellationToken)
    {
        var investidores = await _context.Investidores
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var tickers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var opcoes = new Dictionary<string, OpcaoMercadoConsulta>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var id in investidores)
        {
            var dashboard = await _dashboardHandler.HandleAsync(
                id, cancellationToken);
            foreach (var posicao in dashboard.Posicoes.Where(x => x.Quantidade > 0))
                tickers.Add(posicao.Ticker);

            foreach (var opcao in dashboard.Opcoes
                .Where(x => x.Situacao == "ABERTA" || x.Situacao == "EXECUTADA"))
            {
                tickers.Add(opcao.TickerAtivo);
                opcoes[opcao.TickerOpcao] = new OpcaoMercadoConsulta(
                    opcao.TickerOpcao, opcao.Vencimento, opcao.Strike);
            }
        }

        var atualizado = await _mercadoBrapiService.AtualizarAsync(
            tickers, opcoes.Values, dataLocal, cancellationToken);

        _logger.LogInformation(
            "Atualização semanal do mercado: {Ativos} ativos e {Opcoes} opções com preço disponível.",
            atualizado.Ativos.Count, atualizado.Opcoes.Count);
        return atualizado;
    }

    public async Task ProcessarEnviosAsync(
        DateTime dataLocal,
        CancellationToken cancellationToken,
        AtualizacaoMercadoResultado? mercadoAtualizado = null)
    {
        // Uma consulta à OpenAI por sexta-feira: somente o consolidado administrativo.
        // Os resumos individuais são calculados a partir do dashboard já existente,
        // sem reenviar dados particulares ou consumir outras respostas da IA.
        if (dataLocal.DayOfWeek != DayOfWeek.Friday)
            return;

        var existeDestinatario = await _context.Usuarios
            .AsNoTracking()
            .AnyAsync(x => x.Status == StatusUsuario.Ativo &&
                           x.ReceberRelatorioIa, cancellationToken);
        if (!existeDestinatario)
            return;

        var relatorioAdmin = await ObterHojeAsync(dataLocal, cancellationToken)
            ?? await GerarRelatorioAsync(
                dataLocal, false, null, "TODOS", cancellationToken, mercadoAtualizado);

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
                    "SEMANAL"))
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
                    "SEMANAL"))
                continue;

            var investidoresIds = usuario.Investidores
                .Select(x => x.InvestidorId)
                .Distinct()
                .ToArray();

            if (investidoresIds.Length == 0)
                continue;

            var resumos = new List<RelatorioDiarioIa>();
            try
            {
                foreach (var id in investidoresIds)
                {
                    var relatorio = await GerarResumoInvestidorAsync(
                        dataLocal, id, cancellationToken);
                    resumos.Add(relatorio);
                }

                // Um e-mail por usuário, mesmo quando possui várias carteiras.
                var conteudoAgrupado = string.Join(
                    "\n\n---\n\n",
                    resumos.Select(x => x.Conteudo));
                var envio = new RelatorioDiarioIa(
                    dataLocal.Date, conteudoAgrupado,
                    "Resumo automático (sem chamada à OpenAI)",
                    "INVESTIDOR");
                await EnviarRelatorioAsync(envio, usuario, cancellationToken);
                usuario.RegistrarEnvioRelatorioIa(dataLocal);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao enviar resumo semanal ao usuário {UsuarioId}.",
                    usuario.Id);
            }
        }
    }

    private async Task<RelatorioDiarioIa> GerarResumoInvestidorAsync(
        DateTime dataLocal,
        Guid investidorId,
        CancellationToken cancellationToken)
    {
        var data = dataLocal.Date;
        var salvo = await _context.RelatoriosDiariosIa
            .FirstOrDefaultAsync(x =>
                x.DataReferencia == data &&
                x.Escopo == "INVESTIDOR" &&
                x.InvestidorId == investidorId, cancellationToken);

        if (salvo is not null)
            return salvo;

        var investidor = await _context.Investidores
            .AsNoTracking()
            .Where(x => x.Id == investidorId)
            .Select(x => new { x.Nome })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Investidor não encontrado.");

        var dashboard = await _dashboardHandler.HandleAsync(
            investidorId, cancellationToken);
        var cultura = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        string Dinheiro(decimal numero) => numero.ToString("C2", cultura);
        string Numero(decimal numero) => numero.ToString("N2", cultura);
        string Percentual(decimal numero) => numero.ToString("+0.00;-0.00;0.00", cultura) + "%";
        static string Celula(string valor) => valor.Replace("|", "/").Replace("\n", " ");

        var posicoes = dashboard.Posicoes
            .Where(x => x.Quantidade > 0)
            .OrderBy(x => x.Ticker)
            .ToList();

        var resultadoNaoRealizado = posicoes
            .Where(x => x.PrecoAtual.HasValue)
            .Sum(x => x.Valorizacao);

        var linhas = new List<string>
        {
            $"# Resumo semanal Aportiva — {data:dd/MM/yyyy}",
            $"## Carteira de {Celula(investidor.Nome)}",
            "",
            $"**Patrimônio estimado:** {Dinheiro(dashboard.PatrimonioEstimado)}",
            $"**Caixa informado:** {Dinheiro(dashboard.CaixaDisponivel)}",
            $"**Posições cadastradas:** {posicoes.Count}",
            $"**Resultado não realizado com cotação disponível:** {Dinheiro(resultadoNaoRealizado)}",
            "",
            "## Seus investimentos",
            "| Ativo | Qtd. | PM | Cotação | Valor atual | Resultado R$ | Resultado % |",
            "|---|---:|---:|---:|---:|---:|---:|"
        };

        foreach (var posicao in posicoes)
        {
            var custo = posicao.Quantidade * posicao.PrecoMedio;
            var temCotacao = posicao.PrecoAtual.HasValue;
            var valorAtual = temCotacao || posicao.ValorAtual > 0
                ? Dinheiro(posicao.ValorAtual) : "n/d";
            var resultado = temCotacao
                ? Dinheiro(posicao.Valorizacao) : "n/d";
            var retorno = temCotacao && custo > 0
                ? Percentual(posicao.Valorizacao / custo * 100m) : "n/d";
            linhas.Add(
                $"| {Celula(posicao.Ticker)} | {Numero(posicao.Quantidade)} | " +
                $"{Dinheiro(posicao.PrecoMedio)} | " +
                $"{(temCotacao ? Dinheiro(posicao.PrecoAtual!.Value) : "n/d")} | " +
                $"{valorAtual} | {resultado} | {retorno} |");
        }

        var opcoes = dashboard.Opcoes
            .Where(x => x.Situacao == "ABERTA")
            .OrderBy(x => x.Vencimento)
            .ToList();
        linhas.Add("");
        linhas.Add("## Opções abertas");
        if (opcoes.Count == 0)
            linhas.Add("Nenhuma opção aberta registrada.");
        else
        {
            linhas.Add("| Opção | Ativo | Tipo | Operação | Quantidade | Strike | Vencimento |");
            linhas.Add("|---|---|---|---|---:|---:|---|");
            foreach (var opcao in opcoes)
                linhas.Add(
                    $"| {Celula(opcao.TickerOpcao)} | {Celula(opcao.TickerAtivo)} | " +
                    $"{opcao.TipoOpcao} | {opcao.Natureza} | " +
                    $"{Numero(opcao.Quantidade)} | {Dinheiro(opcao.Strike)} | " +
                    $"{opcao.Vencimento:dd/MM/yyyy} |");
        }

        linhas.Add("");
        linhas.Add("**Observação:** Os valores são estimativas com base nos dados do sistema.");
        linhas.Add("Resultado não realizado não inclui necessariamente proventos, custos e operações passadas.");
        linhas.Add("Cotações podem estar desatualizadas; ativos sem cotação mostram n/d.");
        linhas.Add("Consulte a Aportiva para operações e detalhes adicionais.");

        var relatorio = new RelatorioDiarioIa(
            data, string.Join("\n", linhas),
            "Resumo automático (sem chamada à OpenAI)",
            "INVESTIDOR", investidorId);
        _context.RelatoriosDiariosIa.Add(relatorio);
        await _context.SaveChangesAsync(cancellationToken);
        return relatorio;
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
            "SEMANAL" => hoje > ultimo,
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
        CancellationToken cancellationToken,
        AtualizacaoMercadoResultado? mercadoAtualizado = null)
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

        var mercado = mercadoAtualizado ?? await _mercadoBrapiService.AtualizarAsync(
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
Analise somente os ativos e opções presentes nos dados fornecidos e produza
um briefing diário profissional, claro e amigável em português do Brasil.

Use pesquisa na web para informações atuais, priorizando RI das empresas,
CVM, B3, Banco Central e veículos financeiros confiáveis. Não invente
preços, fatos, dividendos, datas, impactos, relações causais ou fontes.
Diferencie fatos confirmados de interpretação.

Use Markdown e comece com:
# Aportiva Portfolio Brief — DD/MM/AAAA

Em seguida escreva um resumo executivo com o valor estimado da carteira,
principais movimentos, contexto do pregão e a relação desse contexto com as
posições analisadas, sempre que os dados disponíveis permitirem.

Organize o restante preferencialmente em:
## Painel patrimonial completo — obrigatório
Para TODAS as posições fornecidas, mesmo sem notícias, mostre uma tabela Markdown
com Investidor (se escopo TODOS), Ticker, Quantidade, Preço médio (PM),
Capital aplicado estimado (quantidade × PM), Cotação de referência,
Valor de mercado, Resultado não realizado em R$ e em %, e Variação do dia
quando disponível. Use uma linha para CADA posição, sem omissões ou "demais".
Não invente valores ausentes. Quando a cotação atualizada existir, dê
preferência a ela para cálculos de mercado; caso contrário, sinalize
explicitamente a data/base do valor de mercado enviado. Resultado não realizado
= valor de mercado - quantidade × PM; retorno = resultado / capital aplicado × 100,
quando denominador > 0. Não chame esse resultado de rentabilidade total:
dividendos, impostos, taxas e operações anteriores podem não estar refletidos.
Mostre sinal + ou − e valores em reais, com duas casas decimais.
Se o ativo não tiver PM ou valor confiável, escreva "n/d" e não estime.
Trate PREVIDÊNCIA, caixa e cotas de fundos somente conforme os dados existentes;
não atribua PM inexistente nem duplique caixa dentro do patrimônio.

No escopo TODOS, faça obrigatoriamente:
1. Tabela-resumo de TODOS os investidores com patrimônio, caixa,
capital aplicado estimado nas posições e resultado não realizado, se calculável;
2. Tabela consolidada por ticker, somando quantidades e valores e calculando
PM consolidado ponderado pelas quantidades quando houver PM de todas as posições;
3. Uma subseção para CADA investidor, com tabela integral dos respectivos ativos
e subtotais. Nunca misture cotas de titularidades diferentes para fins de caixa,
margem ou exercício de opções.
No escopo INVESTIDOR, exiba só a tabela completa daquele investidor e
seus próprios totais. Preserve rigorosamente o isolamento entre investidores.

## Indicadores e tendências
Quando os dados permitirem, apresente concentração por investidor/ativo,
ganhadores e perdedores da carteira (em R$ e %), posição de caixa,
exposição a FIIs, renda fixa, ações e previdência, distinguindo falta de
classificação de exposição zero. Não invente retorno histórico.
Descreva dados divergentes, com origem/data de cotação e uma indicação
de confiabilidade (confirmado, estimado ou desatualizado).

## Radar de opções em tabela — obrigatório
Uma linha para cada opção aberta/executada presente nos dados, com investidor,
ticker da opção, ativo-objeto, call/put, compra/venda, quantidade,
strike, vencimento, prêmio recebido/pago, cotação de referência, distância
ao strike (%), cobertura/capital necessário estimado e risco observado.
Diferencie quantidade de contratos e quantidade efetiva de ações: não
assuma multiplicador de contratos não fornecido. Para puts vendidas,
mostre desembolso de exercício estimado apenas se a quantidade representar
unidades do ativo. Compare liquidez por investidor, nunca usando caixa
de terceiros para afirmar cobertura.
Quando houver cotação da opção datada, rotule claramente a data e
não trate recompra como executável a mercado sem verificação.

## Layout editorial
Comece com um resumo executivo visualmente escaneável: 4 a 6 métricas
em tabela compacta e, em seguida, 3 a 5 alertas concretos e priorizados.
Use títulos curtos, tabelas Markdown bem formadas, valores alinhados,
símbolos positivos/negativos, resumos ao final de cada investidor e uma
conclusão prática com próximos acompanhamentos. Não produza paredes
de texto. Apresente contexto, números e interpretação separadamente.
Mostre as fontes ao lado dos fatos noticiosos. Não confunda os dados
internos do sistema com pesquisas públicas e evite excesso de notícias
irrelevantes.

## O que mudou na carteira
Use tabela Markdown por ativo quando houver dados suficientes. Mostre preço,
variação e impacto estimado apenas quando for possível derivá-los dos dados.
Quando faltar base de comparação, explique a limitação.

## Por que aconteceu e como chega até a carteira
Explique drivers por ativo ou grupo. Relacione cada notícia às posições
concretas e sinalize quando a causalidade for apenas uma interpretação.

## Opções e strikes
Destaque opções abertas ou executadas, vencimentos, distância dos strikes,
risco de exercício ou atribuição e captura de prêmio quando houver dados.

## Proventos e eventos
Destaque dividendos, JCP, resultados, fatos relevantes e eventos pertinentes.

## Alocação, concentração e riscos
Mostre exposições e concentrações relevantes e as limitações dos dados.

## O que acompanhar
Liste poucos pontos concretos para o próximo pregão ou próximos eventos,
sem dar ordem automática de compra ou venda.

## Contexto de mercado
Inclua apenas indicadores úteis para explicar a carteira, como Ibovespa,
câmbio, juros, CDI, petróleo ou índices externos.

Inclua as fontes pesquisadas próximas das afirmações correspondentes, com
nome da fonte e endereço encontrado na pesquisa. Prefira fontes primárias.
Se algo não puder ser confirmado, diga isso claramente. Evite repetição.
Quando não houver novidade relevante, diga explicitamente.
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
            text = texto,
            html = FormatarEmailHtml(relatorio.Conteudo, nome, relatorio.DataReferencia)
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

    private static string FormatarEmailHtml(
        string markdown, string nome, DateTime dataReferencia)
    {
        static string Escapar(string valor) =>
            System.Net.WebUtility.HtmlEncode(valor);

        static string Inline(string valor)
        {
            var seguro = Escapar(valor);
            return System.Text.RegularExpressions.Regex.Replace(
                seguro,
                @"\*\*(.+?)\*\*",
                "<strong>$1</strong>");
        }

        var html = new StringBuilder();
        html.Append("<!doctype html><html lang=\"pt-BR\"><body style=\"margin:0;background:#f3f6fb;padding:24px;font-family:Arial,sans-serif;color:#172b4d\">");
        html.Append("<main style=\"max-width:840px;margin:auto;background:#fff;padding:24px;border-radius:12px\">");
        html.Append("<h1 style=\"color:#114d90;font-size:24px\">Aportiva | Resumo semanal</h1>");
        html.Append("<p>Olá, ").Append(Escapar(nome)).Append(".</p>");
        html.Append("<p>Referência: ").Append(dataReferencia.ToString("dd/MM/yyyy")).Append("</p>");

        var tabelaAberta = false;
        var indice = 0;
        var linhas = markdown.Split('\n');
        foreach (var original in linhas)
        {
            var linha = original.Trim().TrimEnd('\r');
            if (linha.StartsWith('|') && linha.EndsWith('|'))
            {
                var colunas = linha.Trim('|').Split('|')
                    .Select(x => x.Trim()).ToArray();
                if (colunas.All(x =>
                    System.Text.RegularExpressions.Regex.IsMatch(
                        x, @"^:?-{3,}:?$")))
                    continue;

                if (!tabelaAberta)
                {
                    html.Append("<div style=\"overflow-x:auto;margin:16px 0\"><table style=\"border-collapse:collapse;width:100%;font-size:13px\" cellspacing=\"0\" cellpadding=\"8\">");
                    tabelaAberta = true;
                    indice = 0;
                }

                var tag = indice++ == 0 ? "th" : "td";
                html.Append("<tr>");
                foreach (var coluna in colunas)
                {
                    html.Append('<').Append(tag)
                        .Append(" style=\"border-bottom:1px solid #dce2e8;text-align:left;padding:9px 8px;")
                        .Append(tag == "th" ? "background:#eaf1fa;font-weight:bold;" : "")
                        .Append("\">").Append(Inline(coluna))
                        .Append("</").Append(tag).Append('>');
                }

                html.Append("</tr>");
                continue;
            }

            if (tabelaAberta)
            {
                html.Append("</table></div>");
                tabelaAberta = false;
            }

            if (string.IsNullOrWhiteSpace(linha))
                continue;

            var nivel = linha.TakeWhile(x => x == '#').Count();
            if (nivel is >= 1 and <= 4 && linha.Length > nivel && linha[nivel] == ' ')
            {
                var tag = nivel == 1 ? "h2" : "h3";
                html.Append('<').Append(tag).Append(" style=\"margin-top:24px;color:#114d90\">")
                    .Append(Inline(linha[(nivel + 1)..]))
                    .Append("</").Append(tag).Append('>');
            }
            else if (linha.StartsWith("- ") || linha.StartsWith("* "))
                html.Append("<p style=\"margin:6px 0\">• ").Append(Inline(linha[2..])).Append("</p>");
            else
                html.Append("<p style=\"line-height:1.6\">").Append(Inline(linha)).Append("</p>");
        }

        if (tabelaAberta)
            html.Append("</table></div>");

        html.Append("<p style=\"border-top:1px solid #e5e7eb;padding-top:18px;font-size:12px;color:#64748b\">");
        html.Append("Informações estimadas, sujeitas à atualização de cotações. Aportiva.</p></main></body></html>");
        return html.ToString();
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
