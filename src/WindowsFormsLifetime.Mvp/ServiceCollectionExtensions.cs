using Microsoft.Extensions.DependencyInjection;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime.Mvp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPresenterWithView<TViewInterface, TView, TPresenter>(this IServiceCollection services)
            where TViewInterface : class
            where TView : Control, TViewInterface
            where TPresenter : class
            {
                services.AddSingleton<TViewInterface, TView>();
                services.AddSingleton<TPresenter>();
                return services;
            }
    }
}