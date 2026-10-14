namespace Investimentos.Api.AssistenteIa;

public sealed class AssistenteIaWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AssistenteIaWorker> _logger;
    private readonly Investimentos.Api.ExceptionHandling.LogAplicacaoService _logs;
    private DateTime? _ultimaAtualizacaoLocal;

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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha no envio semanal do Assistente IA.");
                await _logs.RegistrarAsync("Assistente IA", "Worker automatico", ex);
            }

            await Task.Delay(
                TimeSpan.FromMinutes(15),
                stoppingToken);
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

        AtualizacaoMercadoResultado? mercado = null;
        if (_ultimaAtualizacaoLocal != agora.Date)
        {
            mercado = await service.AtualizarMercadoSemanalAsync(
                agora, cancellationToken);
            _ultimaAtualizacaoLocal = agora.Date;
        }

        if (agora.DayOfWeek == DayOfWeek.Friday &&
            GeracaoHabilitada(_configuration, _environment))
        {
            // Único fluxo automático de IA/e-mail: sexta-feira.
            await service.ProcessarEnviosAsync(
                agora, cancellationToken, mercado);
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
