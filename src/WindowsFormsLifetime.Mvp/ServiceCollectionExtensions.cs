using Microsoft.Extensions.DependencyInjection;

namespace WindowsFormsLifetime.Mvp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWindowsFormsLifetime<TStartForm, TView, TPresenter>(this IServiceCollection services,
            Action<WindowsFormsLifetimeOptions>? configure = null)
            where TStartForm : Form, TView
            where TView : class
            where TPresenter : BaseMainFormPresenter<TView>
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
            return services;
        }

        public static IServiceCollection AddPresenterWithView<TViewInterface, TView, TPresenter>(this IServiceCollection services)
            where TViewInterface : class
            where TView : class, TViewInterface
            where TPresenter : class
            {
                services.AddSingleton<TViewInterface, TView>();
                services.AddSingleton<TPresenter>();
                return services;
            }
    }
}