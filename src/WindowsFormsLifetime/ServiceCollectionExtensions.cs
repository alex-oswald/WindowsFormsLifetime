using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WindowsFormsLifetime
{
    public static class ServiceCollectionExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
        public static IServiceCollection AddWindowsFormsLifetime(this IServiceCollection services, Action<WindowsFormsLifetimeOptions> configure, Action<IServiceProvider> preApplicationRunAction = null)
        {
            services.AddSingleton<IHostLifetime, WindowsFormsLifetime>();
            services.AddHostedService(sp =>
            {
                var options = sp.GetRequiredService<IOptions<WindowsFormsLifetimeOptions>>();
                var life = sp.GetRequiredService<IHostApplicationLifetime>();
                var sync = sp.GetRequiredService<WindowsFormsSynchronizationContextProvider>();
                return new WindowsFormsHostedService(options, life, sp, sync, preApplicationRunAction);
            });
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