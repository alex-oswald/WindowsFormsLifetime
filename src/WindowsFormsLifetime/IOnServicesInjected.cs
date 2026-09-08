namespace WindowsFormsLifetime;

/// <summary>
/// Receives notification when a user control's runtime services are ready.
/// </summary>
public interface IOnServicesInjected
{
    /// <summary>
    /// Performs synchronous setup on the UI thread after services have been assigned throughout the control tree.
    /// </summary>
    /// <remarks>
    /// Called once per control, with children notified before their parents.
    /// Services are not available during construction or designer initialization.
    /// </remarks>
    void OnServicesInjected();
}
