using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WindowsFormsLifetime;

public class WindowsFormsHostedService : IHostedService, IDisposable
{
    private CancellationTokenRegistration _applicationStoppingRegistration;
    private bool _threadExceptionHandlerAttached;
    private readonly WindowsFormsLifetimeOptions _options;
    private readonly Action<Exception> _onThreadException;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly WindowsFormsSynchronizationContextProvider _syncContextManager;

    public WindowsFormsHostedService(
        IOptions<WindowsFormsLifetimeOptions> options,
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        WindowsFormsSynchronizationContextProvider syncContextManager,
        Action<IServiceProvider> preApplicationRunAction)
    {
        _options = options.Value;
        _onThreadException = _options.OnThreadException;
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _syncContextManager = syncContextManager;
        PreApplicationRunAction = preApplicationRunAction;
    }

    public Action<IServiceProvider> PreApplicationRunAction { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationStoppingRegistration = _hostApplicationLifetime.ApplicationStopping.Register(state =>
        {
            ((WindowsFormsHostedService)state).OnApplicationStopping();
        },
        this);

        Thread thread = new(StartUiThread)
        {
            Name = "WindowsFormsLifetime UI Thread"
        };
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

        // Don't autoinstall since we are creating our own
        WindowsFormsSynchronizationContext.AutoInstall = false;

        // Create the sync context on our UI thread
        _syncContextManager.SynchronizationContext = new();
        SynchronizationContext.SetSynchronizationContext(_syncContextManager.SynchronizationContext);

        try
        {
            if (_onThreadException != null)
            {
                Application.ThreadException += OnApplicationThreadException;
                _threadExceptionHandlerAttached = true;
            }

            ApplicationContext applicationContext = _serviceProvider.GetService<ApplicationContext>();
            PreApplicationRunAction?.Invoke(_serviceProvider);
            Application.Run(applicationContext);
        }
        finally
        {
            RemoveThreadExceptionHandler();
        }
    }

    private void OnApplicationThreadException(object sender, ThreadExceptionEventArgs e)
    {
        _onThreadException(e.Exception);
    }

    private void OnApplicationStopping()
    {
        ApplicationContext applicationContext = _serviceProvider.GetService<ApplicationContext>();
        Form form = applicationContext.MainForm;

        // If the form is closed then the handle no longer exists
        // We would get an exception trying to invoke from the control when it is already closed
        if (form != null && form.IsHandleCreated)
        {
            // If the host lifetime is stopped, gracefully close and dispose of forms in the service provider
            Action closeAndDispose = () =>
            {
                form.Close();
                form.Dispose();
            };
            form.Invoke(closeAndDispose);
        }
    }

    private void OnApplicationExit(object sender, EventArgs e)
    {
        RemoveThreadExceptionHandler();
        _hostApplicationLifetime.StopApplication();
    }

    private void RemoveThreadExceptionHandler()
    {
        if (_threadExceptionHandlerAttached)
        {
            Application.ThreadException -= OnApplicationThreadException;
            _threadExceptionHandlerAttached = false;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
    public void Dispose()
    {
        Application.ApplicationExit -= OnApplicationExit;
        _applicationStoppingRegistration.Dispose();
    }
}
