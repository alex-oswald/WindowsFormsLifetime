using Microsoft.Extensions.Hosting;
using System.Windows.Forms;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;

namespace WindowsFormsLifetimeTests
{
    public class WindowsFormsLifetimeTestss
    {
        public class Form1 : Form { }

        [Fact]
        public void ServicesAreAvailable()
        {
            using var host = new HostBuilder().UseWindowsFormsLifetime<Form1>().Build();

            Assert.IsType<WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
            Assert.IsType<WindowsFormsHostedService<Form1>>(host.Services.GetService<IHostedService>());
            Assert.NotNull(host.Services.GetService<Form1>());
        }
    }
}