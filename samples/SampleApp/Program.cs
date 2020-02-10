using Microsoft.Extensions.Hosting;
using System;
using System.Windows.Forms;

namespace WindowsFormsLifetime.Sample
{
    static class Program
    {
        static void Main()
        {
            CreateHostBuilder().Build().Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder(Array.Empty<string>())
                .UseWindowsFormsLifetime<Form1>(options =>
                {
                    options.HighDpiMode = HighDpiMode.SystemAware;
                    options.EnableVisualStyles = true;
                    options.CompatibleTextRenderingDefault = false;
                    options.SuppressStatusMessages = false;
                    options.EnableConsoleShutdown = false;
                })
                .ConfigureServices((hostContext, services) =>
                {
                    
                });
    }
}