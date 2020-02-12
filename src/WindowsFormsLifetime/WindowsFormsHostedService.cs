using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OswaldTechnologies.Extensions.Hosting.Lifetime
{
    public class WindowsFormsHostedService<TStartForm> : IHostedService, IDisposable
        where TStartForm : Form
    {
        private CancellationTokenRegistration _applicationStoppingRegistration;
        private readonly WindowsFormsLifetimeOptions _options;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IServiceProvider _serviceProvider;

        public WindowsFormsHostedService(
            IOptions<WindowsFormsLifetimeOptions> options,
            IHostApplicationLifetime hostApplicationLifetime,
            IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationStoppingRegistration = _hostApplicationLifetime.ApplicationStopping.Register(state =>
            {
                ((WindowsFormsHostedService<TStartForm>)state).OnApplicationStopping();
            },
            this);

            var thread = new Thread(StartUiThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void StartUiThread()
        {
            Application.SetHighDpiMode(_options.HighDpiMode);
            if (_options.EnableVisualStyles)
            {
                Application.EnableVisualStyles();
            }
            Application.SetCompatibleTextRenderingDefault(_options.CompatibleTextRenderingDefault);
            Application.ApplicationExit += OnApplicationExit;

            var form = _serviceProvider.GetService<TStartForm>();

            Application.Run(form);
        }

        private void OnApplicationStopping()
        {
            var form = _serviceProvider.GetService<TStartForm>();

            // If the form is closed then the handle no longer exists
            // We would get an exception trying to invoke from the control when it is already closed
            if (form.IsHandleCreated)
            {
                // If the host lifetime is stopped, gracefully close and dispose of forms in the service provider
                form.Invoke(new Action(() =>
                {
                    form.Close();
                    form.Dispose();
                }));
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            _hostApplicationLifetime.StopApplication();
        }

        public void Dispose()
        {
            Application.ApplicationExit -= OnApplicationExit;
            _applicationStoppingRegistration.Dispose();
        }
    }
}