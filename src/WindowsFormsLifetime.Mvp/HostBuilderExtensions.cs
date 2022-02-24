using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WindowsFormsLifetime.Mvp
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseWindowsFormsLifetime<TStartForm, TView, TPresenter>(
            this IHostBuilder hostBuilder, Action<WindowsFormsLifetimeOptions>? configure = null)
            where TStartForm : Form, TView
            where TView : class
            where TPresenter : BaseMainFormPresenter<TView>
            => hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<TStartForm>();
                services.AddSingleton<TView>(provider => provider.GetRequiredService<TStartForm>());
                services.AddSingleton(provider => new ApplicationContext(provider.GetRequiredService<TStartForm>()));
                services.AddSingleton<TPresenter>();
                services.AddWindowsFormsLifetime(configure, serviceProvider =>
                {
                    // Instantiate the presenter before running the application
                    // If we don't do this, a presenter won't exist, and your form won't work
                    var presenter = serviceProvider.GetRequiredService<TPresenter>();
                });
            });
    }
}