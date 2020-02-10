using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OswaldTechnologies.Extensions.Hosting.Lifetime
{
    /// <summary>
    /// Listens for the startup <see cref="Form"/> to close, then initiates shutdown.
    /// </summary>
    public class WindowsFormsLifetime : IHostLifetime, IDisposable
    {
        private CancellationTokenRegistration _applicationStartedRegistration;
        private CancellationTokenRegistration _applicationStoppingRegistration;

        public WindowsFormsLifetime(
            IOptions<WindowsFormsLifetimeOptions> options,
            IHostEnvironment environment,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            ApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            Logger = loggerFactory?.CreateLogger("Microsoft.Hosting.Lifetime") ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private WindowsFormsLifetimeOptions Options { get; }

        private IHostEnvironment Environment { get; }

        private IHostApplicationLifetime ApplicationLifetime { get; }

        private ILogger Logger { get; }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (!Options.SuppressStatusMessages)
            {
                _applicationStartedRegistration = ApplicationLifetime.ApplicationStarted.Register(state =>
                {
                    ((WindowsFormsLifetime)state).OnApplicationStarted();
                },
                this);
                _applicationStoppingRegistration = ApplicationLifetime.ApplicationStopping.Register(state =>
                {
                    ((WindowsFormsLifetime)state).OnApplicationStopping();
                },
                this);
            }

            if (Options.EnableConsoleShutdown)
            {
                Console.CancelKeyPress += OnCancelKeyPress;
            }

            // Windows Forms applications start immediately.
            return Task.CompletedTask;
        }

        private void OnApplicationStarted()
        {
            Logger.LogInformation("Application started. Close the startup Form" + (Options.EnableConsoleShutdown ? " or press Ctrl+C" : string.Empty) + " to shut down.");
            Logger.LogInformation("Hosting environment: {envName}", Environment.EnvironmentName);
            Logger.LogInformation("Content root path: {contentRoot}", Environment.ContentRootPath);
        }

        private void OnApplicationStopping()
        {
            Logger.LogInformation("Application is shutting down...");
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            ApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // There's nothing to do here
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _applicationStartedRegistration.Dispose();
            _applicationStoppingRegistration.Dispose();

            if (Options.EnableConsoleShutdown)
            {
                Console.CancelKeyPress -= OnCancelKeyPress;
            }
        }
    }
}