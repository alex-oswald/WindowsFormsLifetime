using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleApp;

public class TickingHostedService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly TickBag _tickBag;

    public TickingHostedService(ILogger<TickingHostedService> logger, TickBag tickBag)
    {
        _logger = logger;
        _tickBag = tickBag;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(2000, stoppingToken);
            _tickBag.Increment();
            _logger.LogInformation(
                "Tick 2000ms, thread id = {threadId}, thread name = {threadName}, tick value={value}",
                Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, _tickBag.CurrentTick);
        }
    }
}