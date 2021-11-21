using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
{
    public static class ServiceCollectionExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
        public static IServiceCollection AddWindowsFormsLifetime(this IServiceCollection services, Action<WindowsFormsLifetimeOptions> configure)
        {
            services.AddSingleton<IHostLifetime, WindowsFormsLifetime>();
            services.AddHostedService<WindowsFormsHostedService>();
            services.Configure(configure ?? (_ => new WindowsFormsLifetimeOptions()));

            services.AddSingleton<IFormProvider, FormProvider>();

            // Synchronization context
            services.AddSingleton<WindowsFormsSynchronizationContextProvider>();
            services.AddSingleton<IWindowsFormsSynchronizationContextProvider>(sp => sp.GetRequiredService<WindowsFormsSynchronizationContextProvider>());
            services.AddSingleton<IGuiContext>(sp => sp.GetRequiredService<WindowsFormsSynchronizationContextProvider>());

            return services;
        }
    }
}