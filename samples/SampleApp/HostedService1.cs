using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp
{
    public class HostedService1 : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IFormProvider _fp;
        private readonly IGuiContext _guiContext;

        public HostedService1(
            ILogger<HostedService1> logger,
            IFormProvider formProvider,
            IGuiContext guiContext)
        {
            _logger = logger;
            _fp = formProvider;
            _guiContext = guiContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int count = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
                if (count < 5)
                {
                    await _guiContext.InvokeAsync(async () =>
                    {
                        var form = await _fp.GetFormAsync<Form2>();
                        form.Show();
                    });
                }
                count++;
                _logger.LogInformation("HostedService1 Tick 1000ms");
            }
        }
    }
}