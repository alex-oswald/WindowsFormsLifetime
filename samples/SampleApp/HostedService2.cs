namespace SampleApp
{
    public class HostedService2 : BackgroundService
    {
        private readonly ILogger _logger;

        public HostedService2(ILogger<HostedService2> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2000);
                _logger.LogInformation("HostedService2 Tick 2000ms");
            }
        }
    }
}