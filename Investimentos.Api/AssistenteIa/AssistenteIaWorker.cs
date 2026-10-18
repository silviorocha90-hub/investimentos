namespace Investimentos.Api.AssistenteIa;

public sealed class AssistenteIaWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AssistenteIaWorker> _logger;
    private readonly Investimentos.Api.ExceptionHandling.LogAplicacaoService _logs;

    public AssistenteIaWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<AssistenteIaWorker> logger,
        Investimentos.Api.ExceptionHandling.LogAplicacaoService logs)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
        _logs = logs;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecutarSeNecessarioAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == -2)
            {
                _logger.LogWarning(ex,
                    "Azure SQL indisponivel por timeout na conexao. " +
                    "Worker tentara novamente no proximo ciclo; nenhuma cotacao foi marcada como concluida.");
                await RegistrarErroSemInterromperWorkerAsync(
                    "Timeout Azure SQL", ex, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha no ciclo automatico do Assistente IA.");
                await RegistrarErroSemInterromperWorkerAsync(
                    "Worker automatico", ex, stoppingToken);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    // O log de erro tambem utiliza SQL; sua falha nao pode encerrar o worker.
    private async Task RegistrarErroSemInterromperWorkerAsync(
        string operacao, Exception erro, CancellationToken cancellationToken)
    {
        try
        {
            await _logs.RegistrarAsync("Assistente IA", operacao, erro);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception logEx)
        {
            _logger.LogWarning(logEx,
                "Falha ao persistir diagnostico do Assistente IA em LogsAplicacao.");
        }
    }

    private async Task ExecutarSeNecessarioAsync(
        CancellationToken cancellationToken)
    {
        var agora = ObterAgoraSaoPaulo();
        // Preços: uma atualização diária. Envio: retentativas independentes
        // a cada ciclo de 15 minutos enquanto houver destinatários pendentes.
        if (agora.Hour < 19)
            return;

        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider
            .GetRequiredService<AssistenteIaService>();

        // A falha nas cotacoes nao pode impedir o relatorio semanal.
        AtualizacaoMercadoResultado? mercado = null;
        try
        {
            mercado = await service.AtualizarMercadoUmaVezAoDiaAsync(
                agora, "automatica", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Falha na atualizacao automatica de cotacoes; relatorio semanal sera tentado independentemente.");
            await RegistrarErroSemInterromperWorkerAsync(
                "Atualizacao cotacoes", ex, cancellationToken);
            // Evita nova consulta ao SQL/Brapi durante a geracao do relatorio.
            mercado = new AtualizacaoMercadoResultado(
                new Dictionary<string, CotacaoAtivoMercado>(
                    StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, CotacaoOpcaoMercado>(
                    StringComparer.OrdinalIgnoreCase));
        }

        // Sexta-feira apos as 19h, com recuperacao no sabado e domingo.
        // A data de referencia permanece a sexta para evitar relatorios
        // diferentes ou duplicados no mesmo fim de semana.
        if (GeracaoHabilitada(_configuration, _environment) &&
            agora.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            var diasDesdeSexta = agora.DayOfWeek switch
            {
                DayOfWeek.Saturday => 1,
                DayOfWeek.Sunday => 2,
                _ => 0
            };
            var sextaReferencia = agora.AddDays(-diasDesdeSexta);
            await service.ProcessarEnviosAsync(
                sextaReferencia, cancellationToken, mercado);
        }
    }

    public static bool GeracaoHabilitada(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Desenvolvimento nunca executa chamadas pagas, mesmo com flags herdadas.
        return environment.IsProduction() &&
            bool.TryParse(configuration["ASSISTENTE_IA_ATIVO"], out var ativo) &&
            ativo;
    }

    public static DateTime ObterAgoraSaoPaulo()
    {
        var ids =
            new[]
            {
                "America/Sao_Paulo",
                "E. South America Standard Time"
            };

        foreach (var id in ids)
        {
            try
            {
                var zona =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        id);

                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    zona);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }

        return DateTime.UtcNow.AddHours(-3);
    }
}


public sealed class GeracaoRelatorioBackground
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<GeracaoRelatorioBackground> _logger;
    private readonly Investimentos.Api.ExceptionHandling.LogAplicacaoService _logs;
    private readonly object _sync = new();
    private Guid? _jobId;
    private string _estado = "ocioso";
    private string? _erro;
    private Guid? _relatorioId;

    public GeracaoRelatorioBackground(IServiceScopeFactory scopes,
        ILogger<GeracaoRelatorioBackground> logger,
        Investimentos.Api.ExceptionHandling.LogAplicacaoService logs)
    {
        _scopes = scopes;
        _logger = logger;
        _logs = logs;
    }

    private object StatusInterno() => new {
        jobId = _jobId, estado = _estado,
        erro = _erro, relatorioId = _relatorioId
    };

    public object Status()
    {
        lock (_sync) return StatusInterno();
    }

    public object Iniciar()
    {
        lock (_sync)
        {
            if (_estado == "executando") return StatusInterno();
            _jobId = Guid.NewGuid();
            _estado = "executando";
            _erro = null;
            _relatorioId = null;
            var id = _jobId.Value;
            _ = Task.Run(() => ExecutarAsync(id));
            return StatusInterno();
        }
    }

    private async Task ExecutarAsync(Guid id)
    {
        var inicio = System.Diagnostics.Stopwatch.StartNew();
        using var limite = new CancellationTokenSource(TimeSpan.FromMinutes(12));
        try
        {
            using var scope = _scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AssistenteIaService>();
            _logger.LogInformation(
                "Geracao e ENVIO IA MANUAL iniciados pelo Admin: {JobId}", id);
            await _logs.RegistrarEtapaAsync("Iniciar geracao", "Geracao manual iniciada.", id.ToString());
            var relatorio = await service.GerarEEnviarManualAdminAsync(
                AssistenteIaWorker.ObterAgoraSaoPaulo(),
                limite.Token, (etapa, mensagem) =>
                    _logs.RegistrarEtapaAsync(etapa, mensagem, id.ToString()));
            lock (_sync)
            {
                _relatorioId = relatorio.Id;
                _estado = "concluido";
            }
            await _logs.RegistrarEtapaAsync("Concluir geracao", $"Relatorio {relatorio.Id} concluido em {inicio.Elapsed.TotalSeconds:F1}s.", id.ToString());
            _logger.LogInformation("Geracao IA concluida: {JobId}, {RelatorioId}; Duracao: {Segundos}s", id, relatorio.Id, inicio.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geracao IA falhou: {JobId} apos {Segundos}s", id, inicio.Elapsed.TotalSeconds);
            await _logs.RegistrarAsync("Assistente IA", "Gerar relatorio", ex, id.ToString());
            lock (_sync)
            {
                _erro = "Falha ao gerar relatorio. Codigo: " + id;
                _estado = "erro";
            }
        }
    }
}
