using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Windows.Forms;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsLifetimeTests
{
    public class WindowsFormsLifetimeTests
    {
        public class TestForm : Form
        {
            protected override void SetVisibleCore(bool value)
            {
                // Don't flash window when running unit tests
                base.SetVisibleCore(false);

                if (!IsHandleCreated)
                {
                    CreateHandle();
                    OnLoad(EventArgs.Empty);
                }
            }
        }

        public class TestContext : ApplicationContext
        {
            public TestContext(Action<TestContext> onStart = null)
            {
                // Let's invoke this after constructor has been run
                var timer = new Timer { Interval = 1, Enabled = true };
                timer.Tick += (sender, args) =>
                {
                    timer.Enabled = false;
                    onStart?.Invoke(this);
                };
            }
        }

        [Fact]
        public void ServicesAreAvailableWhenUsingForm()
        {
            // given
            var hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestForm>();

            // when
            using var host = hostBuilder.Build();

            // then
            Assert.IsType<WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
            Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
            Assert.NotNull(host.Services.GetService<ApplicationContext>());
            Assert.NotNull(host.Services.GetService<TestForm>());
        }

        [Fact]
        public void ServicesAreAvailableWhenUsingApplicationContext()
        {
            // given
            var hostBuilder = new HostBuilder().UseWindowsFormsLifetimeAppContext<TestContext>();

            // when
            using var host = hostBuilder.Build();

            // then
            Assert.IsType<WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
            Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
            Assert.NotNull(host.Services.GetService<ApplicationContext>());
            Assert.NotNull(host.Services.GetService<TestContext>());
        }

        [Fact]
        public async Task ShouldRunAndCloseForm()
        {
            // given
            using var host = new HostBuilder().UseWindowsFormsLifetime<TestForm>().Build();

            var form = host.Services.GetService<TestForm>();
            form.Load += (sender, args) => form.Invoke(new Action(Application.Exit));

            // when
            await host.RunAsync();

            // then
            // if we are here, nothing has failed
        }

        [Fact]
        public async Task ShouldRunAndCloseFormWhenCancelling()
        {
            // given
            using var host = new HostBuilder().UseWindowsFormsLifetime<TestForm>().Build();
            using var cancelToken = new CancellationTokenSource();

            var form = host.Services.GetService<TestForm>();
            form.Load += (sender, args) => cancelToken.Cancel();

            // when
            await host.RunAsync(cancelToken.Token);

            // then
            // if we are here, nothing has failed
        }

        [Fact]
        public async Task ShouldRunAndCloseApplicationContext()
        {
            // given
            using var host = new HostBuilder()
                .UseWindowsFormsLifetimeAppContext<TestContext>()
                .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(context => Application.Exit()))
                .Build();

            // when
            await host.RunAsync();

            // then
            // if we are here, nothing has failed
        }

        [Fact]
        public async Task ShouldRunAndCloseApplicationContextWhenCancelling()
        {
            // given
            using var cancelToken = new CancellationTokenSource();
            using var host = new HostBuilder()
                .UseWindowsFormsLifetimeAppContext<TestContext>()
                .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(_ => cancelToken.Cancel()))
                .Build();

            // when
            await host.RunAsync(cancelToken.Token);

            // then
            // if we are here, nothing has failed
        }
    }
}