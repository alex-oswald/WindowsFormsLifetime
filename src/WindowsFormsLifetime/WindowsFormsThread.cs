namespace WindowsFormsLifetime;

internal static class WindowsFormsThread
{
    public static WindowsFormsSynchronizationContext GetSynchronizationContext(
        IWindowsFormsSynchronizationContextProvider provider)
        => provider.SynchronizationContext
            ?? throw new InvalidOperationException("The Windows Forms UI thread is not available.");

    public static void EnsureUiThread(IWindowsFormsSynchronizationContextProvider provider, string operation)
    {
        WindowsFormsSynchronizationContext synchronizationContext = GetSynchronizationContext(provider);
        if (!ReferenceEquals(SynchronizationContext.Current, synchronizationContext))
        {
            throw new InvalidOperationException($"{operation} must be called on the Windows Forms UI thread.");
        }
    }
}
