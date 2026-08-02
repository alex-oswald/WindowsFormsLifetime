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

    private sealed class TestDependency
    {
    }

    private sealed class ServiceProviderTestContext : ApplicationContext
    {
        public ServiceProviderTestContext(TestDependency dependency)
        {
            Dependency = dependency;
        }

        public TestDependency Dependency { get; }
    }

    [Fact]
    public void Services_Available_With_Form()
    {
        IHostBuilder hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestForm>();

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
        IHostBuilder hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestContext>();

        using IHost host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        ApplicationContext applicationContext = host.Services.GetRequiredService<ApplicationContext>();
        TestContext testContext = host.Services.GetRequiredService<TestContext>();

        Assert.Same(testContext, applicationContext);
        Assert.NotNull(host.Services.GetService<IFormProvider>());
        Assert.Null(host.Services.GetService<TestForm>());
    }

    [Fact]
    public void Services_Available_With_ApplicationContext_Form()
    {
        IHostBuilder hostBuilder = new HostBuilder().UseWindowsFormsLifetime<TestContext, TestForm>((form) => new TestContext());

        using IHost host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        ApplicationContext applicationContext = host.Services.GetRequiredService<ApplicationContext>();
        TestContext testContext = host.Services.GetRequiredService<TestContext>();

        Assert.Same(testContext, applicationContext);
        Assert.NotNull(host.Services.GetService<TestForm>());
        Assert.NotNull(host.Services.GetService<IFormProvider>());
    }

    [Fact]
    public void Services_Available_With_ApplicationContext_With_ServiceProvider()
    {
        TestDependency dependency = new();
        IHostBuilder hostBuilder = new HostBuilder()
            .ConfigureServices(services => services.AddSingleton<TestDependency>(dependency))
            .UseWindowsFormsLifetime<ServiceProviderTestContext>(
                static provider => new(provider.GetRequiredService<TestDependency>()));

        using IHost host = hostBuilder.Build();

        Assert.IsType<WindowsFormsLifetime.WindowsFormsLifetime>(host.Services.GetService<IHostLifetime>());
        Assert.IsType<WindowsFormsHostedService>(host.Services.GetService<IHostedService>());
        ApplicationContext applicationContext = host.Services.GetRequiredService<ApplicationContext>();
        ServiceProviderTestContext testContext = host.Services.GetRequiredService<ServiceProviderTestContext>();

        Assert.Same(testContext, applicationContext);
        Assert.Same(dependency, testContext.Dependency);
        Assert.NotNull(host.Services.GetService<IFormProvider>());
        Assert.Null(host.Services.GetService<TestForm>());
    }

    [Fact]
    public void Services_Available_With_ApplicationContext_With_ServiceProvider_On_HostApplicationBuilder()
    {
        TestDependency dependency = new();
        HostApplicationBuilder hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.Services.AddSingleton<TestDependency>(dependency);

        IHostApplicationBuilder returnedBuilder = hostBuilder.UseWindowsFormsLifetime<ServiceProviderTestContext>(
            static provider => new(provider.GetRequiredService<TestDependency>()));

        Assert.Same(hostBuilder, returnedBuilder);

        using IHost host = hostBuilder.Build();

        ApplicationContext applicationContext = host.Services.GetRequiredService<ApplicationContext>();
        ServiceProviderTestContext testContext = host.Services.GetRequiredService<ServiceProviderTestContext>();

        Assert.Same(testContext, applicationContext);
        Assert.Same(dependency, testContext.Dependency);
    }

    [Fact]
    public void Services_Available_With_ApplicationContext_Without_Factory_From_ServiceCollection()
    {
        ServiceCollection services = new();
        services.AddWindowsFormsLifetime<TestContext>();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        ApplicationContext applicationContext = serviceProvider.GetRequiredService<ApplicationContext>();
        TestContext testContext = serviceProvider.GetRequiredService<TestContext>();

        Assert.Same(testContext, applicationContext);
    }

    [Fact]
    public async Task Should_Run_And_Close_Form()
    {
        using IHost host = new HostBuilder().UseWindowsFormsLifetime<TestForm>().Build();

        TestForm? form = host.Services.GetService<TestForm>();
        form!.Load += (sender, args) => form.Invoke(new Action(Application.Exit));

        await host.RunAsync();

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_Form_When_Cancelling()
    {
        using IHost host = new HostBuilder().UseWindowsFormsLifetime<TestForm>().Build();
        using CancellationTokenSource cancelToken = new();

        TestForm? form = host.Services.GetService<TestForm>();
        form!.Load += (sender, args) => cancelToken.Cancel();

        await host.RunAsync(cancelToken.Token);

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_ApplicationContext()
    {
        using IHost host = new HostBuilder()
            .UseWindowsFormsLifetime<TestContext>()
            .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(context => Application.Exit()))
            .Build();

        await host.RunAsync();

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Should_Run_And_Close_ApplicationContext_When_Cancelling()
    {
        using CancellationTokenSource cancelToken = new();
        using IHost host = new HostBuilder()
            .UseWindowsFormsLifetime<TestContext>()
            .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(_ => cancelToken.Cancel()))
            .Build();

        await host.RunAsync(cancelToken.Token);

        // If we are here, nothing failed
    }

    [Fact]
    public async Task Invokes_ThreadException_Handler()
    {
        InvalidOperationException expectedException = new();
        TaskCompletionSource<Exception> exceptionHandled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));
        using IHost host = new HostBuilder()
            .UseWindowsFormsLifetime<TestContext>(
                configure: options => options.OnThreadException = exception =>
                {
                    exceptionHandled.TrySetResult(exception);
                    Application.Exit();
                })
            .ConfigureServices(services => services.AddSingleton<Action<TestContext>>(
                new Action<TestContext>(_ => throw expectedException)))
            .Build();

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