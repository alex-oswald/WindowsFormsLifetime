namespace WindowsFormsLifetime;

public interface IGuiContext
{
    void Invoke(Action action);

    TResult Invoke<TResult>(Func<TResult> func);

    Task<TResult> InvokeAsync<TResult>(Func<TResult> func);

    Task<TResult> InvokeAsync<TResult, TInput>(Func<TInput, TResult> func, TInput input);
}

public interface IWindowsFormsSynchronizationContextProvider
{
    /// <summary>
    /// Gets the <see cref="WindowsFormsSynchronizationContext"/> for the UI thread.
    /// </summary>
    WindowsFormsSynchronizationContext SynchronizationContext { get; }
}

public class WindowsFormsSynchronizationContextProvider : IWindowsFormsSynchronizationContextProvider, IGuiContext
{
    public WindowsFormsSynchronizationContext SynchronizationContext { get; internal set; }

    public void Invoke(Action action) => SynchronizationContext.Invoke(action);

    public TResult Invoke<TResult>(Func<TResult> func) => SynchronizationContext.Invoke(func);

    public async Task<TResult> InvokeAsync<TResult>(Func<TResult> func) => await SynchronizationContext.InvokeAsync(func);

    public async Task<TResult> InvokeAsync<TResult, TInput>(Func<TInput, TResult> func, TInput input) => await SynchronizationContext.InvokeAsync(func, input);
}