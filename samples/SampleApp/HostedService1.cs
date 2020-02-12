using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp
{
    public class HostedService1 : BackgroundService
    {
        private readonly ILogger _logger;

        public HostedService1(ILogger<HostedService1> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
                _logger.LogInformation("HostedService1 Tick 1000ms");
            }
        }
    }
}