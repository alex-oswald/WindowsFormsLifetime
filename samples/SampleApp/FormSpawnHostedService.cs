using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;

namespace SampleApp
{
    public class FormSpawnHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IFormProvider _fp;
        private readonly IGuiContext _guiContext;

        public FormSpawnHostedService(
            ILogger<FormSpawnHostedService> logger,
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
                    // Fetch the form here using IFormProvider
                    // The form provider will get the form from the DI container on the gui thread
                    // Then we must invoke the Show method on the gui thread as well using IGuiContext
                    _logger.LogInformation($"GetFormAsync {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}");
                    var form = await _fp.GetFormAsync<Form2>();
                    _guiContext.Invoke(() => form.Show());
                }
                count++;
            }
        }
    }
}