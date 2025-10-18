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

    public async Task<T> GetFormAsync<T>()
        where T : Form
    {
        // We are throttling this because there is only one gui thread
        await _semaphore.WaitAsync();

        var form = await _syncContextManager.SynchronizationContext.InvokeAsync(GetForm<T>);

        _semaphore.Release();

        return form;
    }

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1>(T1 param1) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2>(T1 param1, T2 param2) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2, T3>(T1 param1, T2 param2, T3 param3) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2, param3);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5, param6);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5, param6, param7);

    /// <inheritdoc />
    public async Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) where TForm : Form
        => await CreateFormAsyncWithParameters<TForm>(param1, param2, param3, param4, param5, param6, param7, param8);

    public async Task<T> GetFormAsync<T>(IServiceScope scope) where T : Form
    {
        // We are throttling this because there is only one gui thread
        await _semaphore.WaitAsync();

        var form = await _syncContextManager.SynchronizationContext.InvokeAsync(() => scope.ServiceProvider.GetService<T>());

        _semaphore.Release();

        return form;
    }

    public Task<Form> GetMainFormAsync()
    {
        var applicationContext = _serviceProvider.GetService<ApplicationContext>();
        return Task.FromResult(applicationContext.MainForm);
    }

    public T GetForm<T>() where T : Form
    {
        T form = null;
        var scope = _serviceScopeFactory.CreateScope();
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
        => scope.ServiceProvider.GetService<T>();

    public void Dispose() => _semaphore?.Dispose();

    private TForm CreateFormWithParameters<TForm>(params object[] parameters) where TForm : Form
    {
        return CreateFormWithScope(scope => ActivatorUtilities.CreateInstance<TForm>(scope.ServiceProvider, parameters));
    }

    private TForm CreateFormWithScope<TForm>(Func<IServiceScope, TForm> formFactory) where TForm : Form
    {
        TForm form;
        var scope = _serviceScopeFactory.CreateScope();
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

    private async Task<TForm> CreateFormAsyncWithParameters<TForm>(params object[] parameters) where TForm : Form
    {
        await _semaphore.WaitAsync();

        try
        {
            var form = await _syncContextManager.SynchronizationContext.InvokeAsync(() =>
                CreateFormWithParameters<TForm>(parameters));
            return form;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
