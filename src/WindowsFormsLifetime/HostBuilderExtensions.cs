using Microsoft.Extensions.Hosting;

namespace WindowsFormsLifetime;

public static class HostBuilderExtensions
{
    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="Form"/>,
    /// then waits for the startup form to close before shutting down.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
    public static IHostBuilder UseWindowsFormsLifetime<TStartForm>(
        this IHostBuilder hostBuilder, Action<WindowsFormsLifetimeOptions> configure = null)
        where TStartForm : Form
        => hostBuilder.ConfigureServices(services => services.AddWindowsFormsLifetime<TStartForm>(configure));

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
    public static IHostBuilder UseWindowsFormsLifetime<TAppContext>(
        this IHostBuilder hostBuilder, Func<TAppContext> applicationContextFactory = null, Action<WindowsFormsLifetimeOptions> configure = null)
        where TAppContext : ApplicationContext
        => hostBuilder.ConfigureServices(services => services.AddWindowsFormsLifetime(applicationContextFactory, configure));

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
    public static IHostBuilder UseWindowsFormsLifetime<TAppContext, TStartForm>(
        this IHostBuilder hostBuilder, Func<TStartForm, TAppContext> applicationContextFactory, Action<WindowsFormsLifetimeOptions> configure = null)
        where TAppContext : ApplicationContext
        where TStartForm : Form
        => hostBuilder.ConfigureServices(services => services.AddWindowsFormsLifetime(applicationContextFactory, configure));

#if NET7_0_OR_GREATER
    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="Form"/>,
    /// then waits for the startup form to close before shutting down.
    /// </summary>
    /// <param name="hostAppBuilder">The <see cref="HostApplicationBuilder" /> to configure.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="HostApplicationBuilder"/> for chaining.</returns>
    public static HostApplicationBuilder UseWindowsFormsLifetime<TStartForm>(
        this HostApplicationBuilder hostAppBuilder, Action<WindowsFormsLifetimeOptions> configure = null)
        where TStartForm : Form
    {
        hostAppBuilder.Services.AddWindowsFormsLifetime<TStartForm>(configure);

        return hostAppBuilder;
    }

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="hostAppBuilder">The <see cref="HostApplicationBuilder" /> to configure.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="HostApplicationBuilder"/> for chaining.</returns>
    public static HostApplicationBuilder UseWindowsFormsLifetime<TAppContext>(
        this HostApplicationBuilder hostAppBuilder, Func<TAppContext> applicationContextFactory = null, Action<WindowsFormsLifetimeOptions> configure = null)
        where TAppContext : ApplicationContext
    {
        hostAppBuilder.Services.AddWindowsFormsLifetime(applicationContextFactory, configure);

        return hostAppBuilder;
    }

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="hostAppBuilder">The <see cref="HostApplicationBuilder" /> to configure.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="HostApplicationBuilder"/> for chaining.</returns>
    public static HostApplicationBuilder UseWindowsFormsLifetime<TAppContext, TStartForm>(
        this HostApplicationBuilder hostAppBuilder
        , Func<TStartForm, TAppContext> applicationContextFactory, Action<WindowsFormsLifetimeOptions> configure = null)
        where TAppContext : ApplicationContext
        where TStartForm : Form
    {
        hostAppBuilder.Services.AddWindowsFormsLifetime(applicationContextFactory, configure);

        return hostAppBuilder;
    }
#endif

#if NET8_0_OR_GREATER
    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="Form"/>,
    /// then waits for the startup form to close before shutting down.
    /// </summary>
    /// <param name="hostAppBuilder">The <see cref="IHostApplicationBuilder" /> to configure.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="IHostApplicationBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder UseWindowsFormsLifetime<TStartForm>(
        this IHostApplicationBuilder hostAppBuilder, Action<WindowsFormsLifetimeOptions> configure = null)
        where TStartForm : Form
    {
        hostAppBuilder.Services.AddWindowsFormsLifetime<TStartForm>(configure);

        return hostAppBuilder;
    }

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="hostAppBuilder">The <see cref="IHostApplicationBuilder" /> to configure.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="IHostApplicationBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder UseWindowsFormsLifetime<TAppContext>(
        this IHostApplicationBuilder hostAppBuilder, Func<TAppContext> applicationContextFactory = null, Action<WindowsFormsLifetimeOptions> configure = null)
        where TAppContext : ApplicationContext
    {
        hostAppBuilder.Services.AddWindowsFormsLifetime(applicationContextFactory, configure);

        return hostAppBuilder;
    }

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="hostAppBuilder">The <see cref="IHostApplicationBuilder" /> to configure.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <returns>The same instance of the <see cref="IHostApplicationBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder UseWindowsFormsLifetime<TAppContext, TStartForm>(
        this IHostApplicationBuilder hostAppBuilder
        , Func<TStartForm, TAppContext> applicationContextFactory, Action<WindowsFormsLifetimeOptions> configure = null)
        where TAppContext : ApplicationContext
        where TStartForm : Form
    {
        hostAppBuilder.Services.AddWindowsFormsLifetime(applicationContextFactory, configure);

        return hostAppBuilder;
    }
#endif
}
