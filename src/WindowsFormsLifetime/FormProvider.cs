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
        => InvokeOnUiThreadAsync(() => scope.ServiceProvider.GetService<T>());

    public Task<Form> GetMainFormAsync()
    {
        ApplicationContext applicationContext = _serviceProvider.GetService<ApplicationContext>();
        return Task.FromResult(applicationContext.MainForm);
    }

    public T GetForm<T>() where T : Form
    {
        EnsureUiThread();

        T form = null;
        IServiceScope scope = _serviceScopeFactory.CreateScope();
        try
        {
            form = scope.ServiceProvider.GetService<T>();
            if (form == null)
            {
                scope.Dispose();
            }
            else
            {
                form.Disposed += (s, e) => scope.Dispose();
            }
        }
        catch
        {
            scope.Dispose();
            throw;
        }

        return form;
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
        return scope.ServiceProvider.GetService<T>();
    }

    public void Dispose() => _semaphore?.Dispose();

    private TForm CreateFormWithParameters<TForm>(params object[] parameters) where TForm : Form
    {
        EnsureUiThread();
        return CreateFormWithScope(scope => ActivatorUtilities.CreateInstance<TForm>(scope.ServiceProvider, parameters));
    }

    private TForm CreateFormWithScope<TForm>(Func<IServiceScope, TForm> formFactory) where TForm : Form
    {
        TForm form;
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
                form.Disposed += (_, _) => scope.Dispose();
            }
        }
        catch
        {
            scope.Dispose();
            throw;
        }

        return form;
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
    {
        WindowsFormsSynchronizationContext synchronizationContext = GetUiSynchronizationContext();
        if (!ReferenceEquals(SynchronizationContext.Current, synchronizationContext))
        {
            throw new InvalidOperationException("Synchronous form creation must be called on the Windows Forms UI thread.");
        }
    }

    private WindowsFormsSynchronizationContext GetUiSynchronizationContext()
        => _syncContextManager.SynchronizationContext
            ?? throw new InvalidOperationException("The Windows Forms UI thread is not available.");
}
