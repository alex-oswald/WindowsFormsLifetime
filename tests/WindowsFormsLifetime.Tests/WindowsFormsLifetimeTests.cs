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
            Timer timer = new() { Interval = 1, Enabled = true };
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
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestForm>();

        using IHost host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        Assert.NotNull(host.Services.GetService<ApplicationContext>());
        Assert.NotNull(host.Services.GetService<TestForm>());
        Assert.NotNull(host.Services.GetService<IFormProvider>());
    }

    [Fact]
    public void Services_Available_With_ApplicationContext()
    {
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestContext>();

        using IHost host = hostBuilder.Build();

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
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestContext, TestForm>(_ => new());

        using IHost host = hostBuilder.Build();

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
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestForm>();
        using IHost host = hostBuilder.Build();

        TestForm? form = host.Services.GetService<TestForm>();
        Action exitApplication = Application.Exit;
        form!.Load += (sender, args) => form!.Invoke(exitApplication);

        await host.RunAsync();

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_Form_When_Cancelling()
    {
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestForm>();
        using IHost host = hostBuilder.Build();
        using CancellationTokenSource cancelToken = new();

        TestForm? form = host.Services.GetService<TestForm>();
        form!.Load += (sender, args) => cancelToken.Cancel();

        await host.RunAsync(cancelToken.Token);

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_ApplicationContext()
    {
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestContext>();
        hostBuilder.ConfigureServices(services => services.AddSingleton<Action<TestContext>>(context => Application.Exit()));
        using IHost host = hostBuilder.Build();

        await host.RunAsync();

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_ApplicationContext_When_Cancelling()
    {
        using CancellationTokenSource cancelToken = new();
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestContext>();
        hostBuilder.ConfigureServices(services => services.AddSingleton<Action<TestContext>>(_ => cancelToken.Cancel()));
        using IHost host = hostBuilder.Build();

        await host.RunAsync(cancelToken.Token);

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Invokes_ThreadException_Handler()
    {
        InvalidOperationException expectedException = new();
        TaskCompletionSource<Exception> exceptionHandled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));
        HostBuilder hostBuilder = new();
        hostBuilder.UseWindowsFormsLifetime<TestContext>(
            configure: options => options.OnThreadException = exception =>
            {
                exceptionHandled.TrySetResult(exception);
                Application.Exit();
            });
        Action<TestContext> throwExpectedException = _ => throw expectedException;
        hostBuilder.ConfigureServices(services => services.AddSingleton<Action<TestContext>>(throwExpectedException));
        using IHost host = hostBuilder.Build();

        Task hostTask = host.RunAsync(cancellationTokenSource.Token);
        Task completedTask = await Task.WhenAny(
            exceptionHandled.Task,
            hostTask,
            Task.Delay(TimeSpan.FromSeconds(10)));
        if (completedTask == hostTask)
        {
            await hostTask;
        }

        Assert.Same(exceptionHandled.Task, completedTask);

        Exception actualException = await exceptionHandled.Task;
        await hostTask.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Same(expectedException, actualException);
    }
}