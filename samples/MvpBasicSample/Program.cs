using Microsoft.Extensions.Hosting;
using WindowsFormsLifetime.Mvp;

namespace MvpBasicSample
{
    internal static class Program
    {
        static void Main()
        {
            CreateHostBuilder().Build().Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder(Array.Empty<string>())
                .UseWindowsFormsLifetime<MainForm, IMainView, MainFormPresenter>()
                .ConfigureServices((hostContext, services) =>
                {

                });
    }
}