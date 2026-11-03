namespace TransAndes.Workers;

/// <summary>
/// Esqueleto del worker RNDC (ADR-002, tasks T025). Publicará el outbox en Service Bus
/// y procesará los envíos con reintentos e idempotencia. Por ahora solo late.
/// </summary>
public sealed partial class RndcWorker : BackgroundService
{
    // Campo explícito: el generador [LoggerMessage] de .NET 8 no admite parámetros de constructor primario.
    private readonly ILogger<RndcWorker> _logger;

    public RndcWorker(ILogger<RndcWorker> logger) => _logger = logger;

    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogLatido(DateTimeOffset.UtcNow);
            await Task.Delay(Intervalo, stoppingToken).ConfigureAwait(false);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Worker RNDC activo: {Momento}")]
    private partial void LogLatido(DateTimeOffset momento);
}
