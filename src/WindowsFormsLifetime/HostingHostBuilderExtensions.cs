using Microsoft.Extensions.DependencyInjection;
using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;
using System;
using System.Windows.Forms;

namespace Microsoft.Extensions.Hosting
{
    public static class HostingHostBuilderExtensions
    {
        /// <summary>
        /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="Form"/>, then waits for the startup form to close before shutting down.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
        /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetime"/>.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseWindowsFormsLifetime<TStartForm>(this IHostBuilder hostBuilder, Action<WindowsFormsLifetimeOptions> configure = null)
            where TStartForm : Form
            => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton<TStartForm>()
                    .AddSingleton(provider => new ApplicationContext(provider.GetRequiredService<TStartForm>()))
                    .AddWindowsFormsLifetime(configure);
            });

        /// <summary>
        /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>, then waits for the startup context to close before shutting down.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
        /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetime"/>.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseWindowsFormsLifetimeAppContext<TAppContext>(this IHostBuilder hostBuilder, Action<WindowsFormsLifetimeOptions> configure = null)
            where TAppContext : ApplicationContext
            => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton<TAppContext>()
                    .AddSingleton<ApplicationContext>(provider => provider.GetRequiredService<TAppContext>())
                    .AddWindowsFormsLifetime(configure);
            });

        /// <summary>
        /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>, then waits for the startup context to close before shutting down.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
        /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
        /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetime"/>.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseWindowsFormsLifetimeAppContext<TAppContext, TStartForm>(this IHostBuilder hostBuilder, Func<TStartForm, ApplicationContext> applicationContextFactory, Action<WindowsFormsLifetimeOptions> configure = null)
            where TAppContext : ApplicationContext
            where TStartForm : Form
            => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton<TStartForm>()
                    .AddSingleton<TAppContext>()
                    .AddSingleton<ApplicationContext>(provider =>
                    {
                        var startForm = provider.GetRequiredService<TStartForm>();
                        return applicationContextFactory(startForm);
                    })
                    .AddWindowsFormsLifetime(configure);
            });

        private static IServiceCollection AddWindowsFormsLifetime(this IServiceCollection services, Action<WindowsFormsLifetimeOptions> configure)
            => services
                .AddSingleton<IHostLifetime, WindowsFormsLifetime>()
                .AddSingleton<FormProvider>()
                .AddSingleton<IFormProvider>(sp => sp.GetRequiredService<FormProvider>())
                .AddSingleton<IGuiContext>(sp => sp.GetRequiredService<FormProvider>())
                .AddHostedService<WindowsFormsHostedService>()
                .Configure(configure ?? (_ => new WindowsFormsLifetimeOptions()));
    }
}