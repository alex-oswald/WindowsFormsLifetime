using Microsoft.Extensions.DependencyInjection;

namespace WindowsFormsLifetime.Mvp;

public static class ServiceCollectionExtensions
{
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