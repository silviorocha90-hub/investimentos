namespace Investimentos.Api.AssistenteIa;

public sealed class AssistenteIaWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssistenteIaWorker> _logger;

    public AssistenteIaWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AssistenteIaWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
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
                    "Falha no Assistente IA diário.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(15),
                stoppingToken);
        }
    }

    private async Task ExecutarSeNecessarioAsync(
        CancellationToken cancellationToken)
    {
        if (!bool.TryParse(
                _configuration["ASSISTENTE_IA_ATIVO"],
                out var ativo) ||
            !ativo)
            return;

        var hora =
            int.TryParse(
                _configuration["ASSISTENTE_IA_HORA"],
                out var configurada)
                ? Math.Clamp(configurada, 0, 23)
                : 19;

        var agora =
            ObterAgoraSaoPaulo();

        if (agora.Hour < hora)
            return;

        using var scope =
            _scopeFactory.CreateScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    AssistenteIaService>();

        await service.ProcessarEnviosAsync(
            agora,
            cancellationToken);
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
