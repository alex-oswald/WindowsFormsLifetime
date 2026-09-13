using Microsoft.Extensions.DependencyInjection;

namespace WindowsFormsLifetime;

public class FormProvider : IFormProvider
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceProvider _serviceProvider;
    private readonly IWindowsFormsSynchronizationContextProvider _syncContextManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FormProvider(
        IServiceProvider serviceProvider,
        IWindowsFormsSynchronizationContextProvider syncContextManager,
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceProvider = serviceProvider;
        _syncContextManager = syncContextManager;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task<T> GetFormAsync<T>()
        where T : Form
        => InvokeOnUiThreadAsync(GetForm<T>);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1>(T1 param1) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2>(T1 param1, T2 param2) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2, T3>(T1 param1, T2 param2, T3 param3) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2, param3);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5, param6);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5, param6, param7);

    /// <inheritdoc />
    public Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) where TForm : Form
        => CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5, param6, param7, param8);

    public Task<T> GetFormAsync<T>(IServiceScope scope) where T : Form
        => InvokeOnUiThreadAsync(() => GetForm<T>(scope));

    public Task<Form> GetMainFormAsync()
    {
        ApplicationContext applicationContext = _serviceProvider.GetService<ApplicationContext>();
        return Task.FromResult(applicationContext.MainForm);
    }

    public T GetForm<T>() where T : Form
    {
        EnsureUiThread();
        return CreateFormWithScope(scope => scope.ServiceProvider.GetService<T>());
    }

    /// <inheritdoc />
    public TForm GetForm<TForm, T1>(T1 param1) where TForm : Form
        => CreateFormWithParameters<TForm>(param1);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2>(T1 param1, T2 param2) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2, T3>(T1 param1, T2 param2, T3 param3) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2, param3);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2, param3, param4);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2, param3, param4, param5);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2, param3, param4, param5, param6);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2, param3, param4, param5, param6, param7);

    /// <inheritdoc />
    public TForm GetForm<TForm, T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) where TForm : Form
        => CreateFormWithParameters<TForm>(param1, param2, param3, param4, param5, param6, param7, param8);

    public T GetForm<T>(IServiceScope scope) where T : Form
    {
        EnsureUiThread();
        T form = scope.ServiceProvider.GetService<T>();
        InjectControlServices(form, scope.ServiceProvider);
        return form;
    }

    public void Dispose() => _semaphore?.Dispose();

    private TForm CreateFormWithParameters<TForm>(params object[] parameters) where TForm : Form
    {
        EnsureUiThread();
        return CreateFormWithScope(scope => ActivatorUtilities.CreateInstance<TForm>(scope.ServiceProvider, parameters), ownsForm: true);
    }

    private TForm CreateFormWithScope<TForm>(Func<IServiceScope, TForm> formFactory, bool ownsForm = false) where TForm : Form
    {
        TForm form = null;
        IServiceScope scope = _serviceScopeFactory.CreateScope();
        try
        {
            form = formFactory(scope);
            if (form == null)
            {
                scope.Dispose();
            }
            else
            {
                InjectControlServices(form, scope.ServiceProvider);
                form.Disposed += (_, _) => scope.Dispose();
            }
        }
        catch
        {
            try
            {
                if (ownsForm)
                {
                    form?.Dispose();
                }
            }
            finally
            {
                scope.Dispose();
            }

            throw;
        }

        return form;
    }

    private void InjectControlServices(Form form, IServiceProvider services)
    {
        if (form != null)
        {
            _serviceProvider.GetService<IControlServiceInjector>()?.Inject(form, services);
        }
    }

    private Task<TForm> CreateFormAsyncWithParameters<TForm>(params object[] parameters) where TForm : Form
        => InvokeOnUiThreadAsync(() => CreateFormWithParameters<TForm>(parameters));

    private async Task<T> InvokeOnUiThreadAsync<T>(Func<T> formFactory)
    {
        WindowsFormsSynchronizationContext synchronizationContext = GetUiSynchronizationContext();
        await _semaphore.WaitAsync();

        try
        {
            return await synchronizationContext.InvokeAsync(formFactory);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void EnsureUiThread()
        => WindowsFormsThread.EnsureUiThread(_syncContextManager, "Synchronous form creation");

    private WindowsFormsSynchronizationContext GetUiSynchronizationContext()
        => WindowsFormsThread.GetSynchronizationContext(_syncContextManager);
}
