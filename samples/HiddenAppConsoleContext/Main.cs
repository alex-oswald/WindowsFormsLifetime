using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Specshell.WinForm.HiddenForm;

namespace HiddenAppConsoleContext
{
    public class Main : MainHiddenForm
    {
        private readonly ILogger<Main> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Main(ILogger<Main> logger)
        {
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            Load += OnHiddenFormLoad;
            Closing += OnHiddenFormClosing;
        }

        public Form Form => this;

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Execute");
            var stoppingToken = _cancellationTokenSource.Token;
            var timeSpan = TimeSpan.FromSeconds(1);
            var count = 1;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Do work {Count}", count++); // Simulate work
                await Task.Delay(timeSpan, stoppingToken);
            }
        }

        public async Task ConnectAsync()
        {
            _logger.LogInformation("Connect");
            await Task.Delay(TimeSpan.FromSeconds(1)); // Simulate work
        }

        public async Task DisconnectAsync()
        {
            _logger.LogInformation("Disconnect");
            await Task.Delay(TimeSpan.FromSeconds(1)); // Simulate work
        }

        public async void OnHiddenFormClosing(object sender, CancelEventArgs e)
        {
            _logger.LogInformation("Form closing");
            await DisconnectAsync();
        }

        public async void OnHiddenFormLoad(object? sender, EventArgs e)
        {
            _logger.LogInformation("Form load");
            await ConnectAsync();
            await ExecuteAsync();
        }
    }
}