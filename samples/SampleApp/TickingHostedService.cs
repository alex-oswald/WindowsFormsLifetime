using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleApp;

public class TickingHostedService : BackgroundService
{
    private readonly ILogger _logger;

    public TickingHostedService(ILogger<TickingHostedService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(2000);
            _logger.LogInformation($"Tick 2000ms {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}");
        }
    }
}