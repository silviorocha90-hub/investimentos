namespace Investimentos.Api.AssistenteIa;

public sealed class AssistenteIaWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AssistenteIaWorker> _logger;
    private DateTime? _ultimaAtualizacaoLocal;

    public AssistenteIaWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<AssistenteIaWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
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
            }

            await Task.Delay(
                TimeSpan.FromMinutes(15),
                stoppingToken);
        }
    }

    private async Task ExecutarSeNecessarioAsync(
        CancellationToken cancellationToken)
    {
        if (!GeracaoHabilitada(_configuration, _environment))
            return;

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

        if (agora.DayOfWeek == DayOfWeek.Friday)
        {
            await service.ForcarRelatorioDeHojeAsync(
                agora, cancellationToken);
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
