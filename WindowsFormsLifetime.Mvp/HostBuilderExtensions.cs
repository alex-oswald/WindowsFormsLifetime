using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WindowsFormsLifetime.Mvp
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddMvpServices(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IEventService, EventService>();
            });

            return builder;
        }
    }
}