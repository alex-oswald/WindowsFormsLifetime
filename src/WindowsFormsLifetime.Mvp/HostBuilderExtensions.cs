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
            => hostBuilder.ConfigureServices((context, services) => services.AddWindowsFormsLifetime<TStartForm, TView, TPresenter>());
    }
}