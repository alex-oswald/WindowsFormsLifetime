using Microsoft.Extensions.DependencyInjection;
using OswaldTechnologies.Extensions.Hosting.Lifetime;
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
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseWindowsFormsLifetime<TStartForm>(this IHostBuilder hostBuilder)
            where TStartForm : Form
            => UseWindowsFormsLifetime<TStartForm>(hostBuilder, _ => new WindowsFormsLifetimeOptions());

        /// <summary>
        /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="Form"/>, then waits for the startup form to close before shutting down.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder" /> to configure.</param>
        /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetime"/>.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseWindowsFormsLifetime<TStartForm>(this IHostBuilder hostBuilder, Action<WindowsFormsLifetimeOptions> configure)
            where TStartForm : Form
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IHostLifetime, WindowsFormsLifetime>();
                services.AddSingleton<IHostedService, WindowsFormsHostedService<TStartForm>>();
                services.AddSingleton<TStartForm>();
                services.Configure(configure);
            });

            return hostBuilder;
        }
    }
} 