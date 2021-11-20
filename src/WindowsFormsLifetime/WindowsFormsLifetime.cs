using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
{
    /// <summary>
    /// Listens for the startup <see cref="Form"/> to close, then initiates shutdown.
    /// </summary>
    public class WindowsFormsLifetime : IHostLifetime, IDisposable
    {
        private CancellationTokenRegistration _applicationStartedRegistration;
        private CancellationTokenRegistration _applicationStoppingRegistration;
        private readonly WindowsFormsLifetimeOptions _options;
        private readonly IHostEnvironment _environment;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;

        public WindowsFormsLifetime(
            IOptions<WindowsFormsLifetimeOptions> options,
            IHostEnvironment environment,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _applicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            _logger = loggerFactory?.CreateLogger("Microsoft.Hosting.Lifetime") ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (!_options.SuppressStatusMessages)
            {
                _applicationStartedRegistration = _applicationLifetime.ApplicationStarted.Register(state =>
                {
                    ((WindowsFormsLifetime)state).OnApplicationStarted();
                },
                this);
                _applicationStoppingRegistration = _applicationLifetime.ApplicationStopping.Register(state =>
                {
                    ((WindowsFormsLifetime)state).OnApplicationStopping();
                },
                this);
            }

            if (_options.EnableConsoleShutdown)
            {
                Console.CancelKeyPress += OnCancelKeyPress;
            }

            // Windows Forms applications start immediately.
            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
        private void OnApplicationStarted()
        {
            _logger.LogInformation("Application started. Close the startup Form" + (_options.EnableConsoleShutdown ? " or press Ctrl+C" : string.Empty) + " to shut down.");
            _logger.LogInformation("Hosting environment: {envName}", _environment.EnvironmentName);
            _logger.LogInformation("Content root path: {contentRoot}", _environment.ContentRootPath);
        }

        private void OnApplicationStopping()
        {
            _logger.LogInformation("Application is shutting down...");
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _applicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // There's nothing to do here
            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public void Dispose()
        {
            _applicationStartedRegistration.Dispose();
            _applicationStoppingRegistration.Dispose();

            if (_options.EnableConsoleShutdown)
            {
                Console.CancelKeyPress -= OnCancelKeyPress;
            }
        }
    }
}