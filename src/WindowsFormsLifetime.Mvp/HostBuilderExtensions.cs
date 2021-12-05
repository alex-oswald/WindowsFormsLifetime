using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime.Mvp
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseWindowsFormsLifetime<TStartForm, TView, TPresenter>(
            this IHostBuilder hostBuilder, Action<WindowsFormsLifetimeOptions>? configure = null)
            where TStartForm : Form, TView
            where TView : class
            where TPresenter : class
            => hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<TStartForm>();
                services.AddSingleton(provider => new ApplicationContext(provider.GetRequiredService<TStartForm>()));
                services.AddSingleton<TPresenter>();
                services.AddWindowsFormsLifetime(configure);
            });
    }
}