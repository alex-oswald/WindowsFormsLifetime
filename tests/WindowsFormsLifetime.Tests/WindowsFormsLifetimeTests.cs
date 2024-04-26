using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows.Forms;
using WindowsFormsLifetime;
using Xunit;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsLifetimeTests;

// Put both test classes into the same collection so that their tests are not run in parallel.
// Otherwise tests fail if both tests run a Host concurrently.
[Collection("Host tests")]
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
        public TestContext(Action<TestContext>? onStart = null)
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
    public void Services_Available_With_Form()
    {
        var hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestForm>();

        using var host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        Assert.NotNull(host.Services.GetService<ApplicationContext>());
        Assert.NotNull(host.Services.GetService<TestForm>());
        Assert.NotNull(host.Services.GetService<IFormProvider>());
    }

    [Fact]
    public void Services_Available_With_ApplicationContext()
    {
        var hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestContext>();

        using var host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        Assert.NotNull(host.Services.GetService<ApplicationContext>());
        Assert.NotNull(host.Services.GetService<TestContext>());
        Assert.NotNull(host.Services.GetService<IFormProvider>());
        Assert.Null(host.Services.GetService<TestForm>());
    }

    [Fact]
    public void Services_Available_With_ApplicationContext_Form()
    {
        var hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestContext, TestForm>((form) => new TestContext());

        using var host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        Assert.NotNull(host.Services.GetService<ApplicationContext>());
        Assert.NotNull(host.Services.GetService<TestContext>());
        Assert.NotNull(host.Services.GetService<TestForm>());
        Assert.NotNull(host.Services.GetService<IFormProvider>());
    }

    [Fact]
    public async Task Should_Run_And_Close_Form()
    {
        using var host = new HostBuilder().UseWindowsFormsLifetime<TestForm>().Build();

        var form = host.Services.GetService<TestForm>();
        form!.Load += (sender, args) => form.Invoke(new Action(Application.Exit));

        await host.RunAsync();

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_Form_When_Cancelling()
    {
        using var host = new HostBuilder().UseWindowsFormsLifetime<TestForm>().Build();
        using var cancelToken = new CancellationTokenSource();

        var form = host.Services.GetService<TestForm>();
        form!.Load += (sender, args) => cancelToken.Cancel();

        await host.RunAsync(cancelToken.Token);

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_ApplicationContext()
    {
        using var host = new HostBuilder()
            .UseWindowsFormsLifetime<TestContext>()
            .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(context => Application.Exit()))
            .Build();

        await host.RunAsync();

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_ApplicationContext_When_Cancelling()
    {
        using var cancelToken = new CancellationTokenSource();
        using var host = new HostBuilder()
            .UseWindowsFormsLifetime<TestContext>()
            .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(_ => cancelToken.Cancel()))
            .Build();

        await host.RunAsync(cancelToken.Token);

        // If we are here, nothing failed
    }
}