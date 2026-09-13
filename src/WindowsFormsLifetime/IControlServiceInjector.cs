namespace WindowsFormsLifetime;

/// <summary>
/// Initializes runtime services on existing user controls without replacing designer-created instances.
/// </summary>
public interface IControlServiceInjector
{
    /// <summary>
    /// Injects required services and notifies uninitialized user controls in a control tree.
    /// </summary>
    /// <param name="root">The existing root control whose tree should be initialized.</param>
    /// <param name="services">The provider that owns the injected services.</param>
    /// <remarks>
    /// Must be called on the library's UI thread after construction and before displaying the controls.
    /// Does not own or dispose the controls or provider. Initialized controls are not rebound on later calls.
    /// Setter and callback failures are not transactional; reconstruct the affected controls instead of retrying.
    /// </remarks>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ObjectDisposedException">A control being initialized is disposed.</exception>
    /// <exception cref="InvalidOperationException">
    /// The UI context is unavailable, the caller is on the wrong thread, or initialization cannot complete.
    /// </exception>
    void Inject(Control root, IServiceProvider services);
}
