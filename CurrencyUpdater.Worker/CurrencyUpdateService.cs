using CurrencyUpdater.Worker.Commands;

namespace CurrencyUpdater.Worker;

public class CurrencyUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CurrencyUpdateService> _logger;
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromHours(1);

    public CurrencyUpdateService(
        IServiceScopeFactory scopeFactory,
        ILogger<CurrencyUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(UpdateInterval);

        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<UpdateCurrenciesCommandHandler>();
                await handler.HandleAsync(new UpdateCurrenciesCommand(), stoppingToken);
            }
            catch (Exception) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Currency update cancelled due to shutdown");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update currencies");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
