namespace AppContext;

public partial class HiddenForm : Form
{
    private readonly ILogger<HiddenForm> _logger;
    private readonly IHostApplicationLifetime _hostLifetime;

    public HiddenForm(
        ILogger<HiddenForm> logger,
        IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        _hostLifetime = hostLifetime;
        Load += OnLoad;
        FormClosing += OnFormClosing;
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _logger.LogInformation("Form closing");
    }

    private async void OnLoad(object? sender, EventArgs e)
    {
        // Example of a hidden main form
        Visible = false;
        ShowInTaskbar = false;
        _logger.LogInformation("Form load");
        await ExecuteAsync();
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Execute");
        int count = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (count == 10)
            {
                _hostLifetime.StopApplication();
            }
            _logger.LogInformation("Do work {Count}", count++);
            await Task.Delay(1000, stoppingToken);
        }
    }
}