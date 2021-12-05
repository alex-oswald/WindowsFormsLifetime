using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WindowsFormsLifetime
{
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
            => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton<TStartForm>()
                    .AddSingleton(provider => new ApplicationContext(provider.GetRequiredService<TStartForm>()))
                    .AddWindowsFormsLifetime(configure);
            });

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
            => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                if (applicationContextFactory is not null)
                {
                    services.AddSingleton<TAppContext>(provider =>
                    {
                        return applicationContextFactory();
                    });
                }
                else
                {
                    services.AddSingleton<TAppContext>();
                }
                services.AddSingleton<ApplicationContext>(provider => provider.GetRequiredService<TAppContext>());
                services.AddWindowsFormsLifetime(configure);
            });

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
            => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<TStartForm>();
                services.AddSingleton<TAppContext>(provider =>
                {
                    var startForm = provider.GetRequiredService<TStartForm>();
                    return applicationContextFactory(startForm);
                });
                services.AddSingleton<ApplicationContext>(provider => provider.GetRequiredService<TAppContext>());
                services.AddWindowsFormsLifetime(configure);
            });
    }
}