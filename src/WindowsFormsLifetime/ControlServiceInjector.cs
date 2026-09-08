using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WindowsFormsLifetime;

internal sealed class ControlServiceInjector : IControlServiceInjector
{
    private readonly IWindowsFormsSynchronizationContextProvider _syncContextProvider;
    private readonly Dictionary<Type, PropertyInfo[]> _properties = new();
    private readonly ConditionalWeakTable<UserControl, InitializationState> _states = new();

    public ControlServiceInjector(IWindowsFormsSynchronizationContextProvider syncContextProvider)
    {
        _syncContextProvider = syncContextProvider;
    }

    public void Inject(Control root, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(services);
        WindowsFormsThread.EnsureUiThread(_syncContextProvider, "Control service injection");
        ObjectDisposedException.ThrowIf(root.IsDisposed, root);

        List<PendingControl> pending = new();
        CollectControls(root, pending);
        foreach (PendingControl item in pending)
        {
            item.State.Status = InitializationStatus.Initializing;
        }

        bool assignmentsStarted = false;
        try
        {
            // Resolve the whole tree before running service property setters or readiness callbacks.
            foreach (PendingControl item in pending)
            {
                for (int index = 0; index < item.Properties.Length; index++)
                {
                    PropertyInfo property = item.Properties[index];
                    try
                    {
                        item.Values[index] = services.GetRequiredService(property.PropertyType);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(
                            $"Could not resolve service for '{item.Control.GetType().FullName}.{property.Name}'.", exception);
                    }
                }
            }

            assignmentsStarted = true;
            foreach (PendingControl item in pending)
            {
                ObjectDisposedException.ThrowIf(item.Control.IsDisposed, item.Control);
                for (int index = 0; index < item.Properties.Length; index++)
                {
                    PropertyInfo property = item.Properties[index];
                    try
                    {
                        property.SetValue(item.Control, item.Values[index]);
                    }
                    catch (TargetInvocationException exception) when (exception.InnerException != null)
                    {
                        throw new InvalidOperationException(
                            $"Could not assign service to '{item.Control.GetType().FullName}.{property.Name}'.", exception.InnerException);
                    }
                }
            }

            foreach (PendingControl item in pending)
            {
                ObjectDisposedException.ThrowIf(item.Control.IsDisposed, item.Control);
                if (item.Control is IOnServicesInjected notification)
                {
                    try
                    {
                        notification.OnServicesInjected();
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(
                            $"Service initialization callback failed for '{item.Control.GetType().FullName}'.", exception);
                    }
                }

                ObjectDisposedException.ThrowIf(item.Control.IsDisposed, item.Control);
                item.State.Status = InitializationStatus.Initialized;
            }
        }
        finally
        {
            foreach (PendingControl item in pending)
            {
                if (item.State.Status == InitializationStatus.Initializing)
                {
                    // Only resolution failures can be retried without repeating control side effects.
                    item.State.Status = assignmentsStarted ? InitializationStatus.Failed : InitializationStatus.Uninitialized;
                }
            }
        }
    }

    private void CollectControls(Control control, List<PendingControl> pending)
    {
        ObjectDisposedException.ThrowIf(control.IsDisposed, control);
        foreach (Control child in control.Controls)
        {
            CollectControls(child, pending);
        }

        if (control is not UserControl userControl)
        {
            return;
        }

        InitializationState state = _states.GetValue(userControl, static _ => new InitializationState());
        switch (state.Status)
        {
            case InitializationStatus.Initialized:
                return;
            case InitializationStatus.Initializing:
                throw new InvalidOperationException($"Service initialization is already in progress for '{control.GetType().FullName}'.");
            case InitializationStatus.Failed:
                throw new InvalidOperationException($"Service initialization previously failed for '{control.GetType().FullName}'. Create a new control before retrying.");
        }

        PropertyInfo[] properties = GetProperties(control.GetType());
        if (properties.Length > 0 || control is IOnServicesInjected)
        {
            pending.Add(new PendingControl(userControl, properties, state));
        }
    }

    private PropertyInfo[] GetProperties(Type controlType)
    {
        if (_properties.TryGetValue(controlType, out PropertyInfo[] cached))
        {
            return cached;
        }

        List<PropertyInfo> properties = new();
        HashSet<MethodInfo> overrides = new();
        for (Type type = controlType; type != typeof(UserControl); type = type.BaseType)
        {
            foreach (PropertyInfo property in type.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                MethodInfo accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
                if (accessor != null && accessor.IsVirtual && !overrides.Add(accessor.GetBaseDefinition()))
                {
                    continue;
                }

                if (!Attribute.IsDefined(property, typeof(InjectServiceAttribute), true))
                {
                    continue;
                }

                MethodInfo setter = property.GetSetMethod(true);
                if (setter == null || !setter.IsPublic || setter.IsStatic || property.GetIndexParameters().Length != 0
                    || setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
                {
                    throw new InvalidOperationException(
                        $"Injected property '{controlType.FullName}.{property.Name}' must be a public instance property with a public, non-init setter and no index parameters.");
                }

                properties.Add(property);
            }
        }

        PropertyInfo[] result = properties.ToArray();
        _properties.Add(controlType, result);
        return result;
    }

    private enum InitializationStatus
    {
        Uninitialized,
        Initializing,
        Initialized,
        Failed
    }

    private sealed class InitializationState
    {
        public InitializationStatus Status { get; set; }
    }

    private sealed class PendingControl(UserControl control, PropertyInfo[] properties, InitializationState state)
    {
        public UserControl Control { get; } = control;
        public PropertyInfo[] Properties { get; } = properties;
        public object[] Values { get; } = new object[properties.Length];
        public InitializationState State { get; } = state;
    }
}
