using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows.Forms;
using WindowsFormsLifetime;
using Xunit;
using System.ComponentModel;

namespace WindowsFormsLifetimeTests;

// Put both test classes into the same collection so that their tests are not run in parallel.
// Otherwise tests fail if both tests run a Host concurrently.
[Collection("Host tests")]
public class FormProviderTests(FormProviderTests.HostFixture host) : IClassFixture<FormProviderTests.HostFixture>
{
    private readonly HostFixture _host = host;
    private static readonly string[] ParameterValues = ["one", "two", "three", "four", "five", "six", "seven", "eight"];

    private IGuiContext GuiContext => _host.Host.Services.GetRequiredService<IGuiContext>();

    public class HostFixture : IDisposable
    {
        public IHost Host { get; init; }
        public CancellationTokenSource TokenSource { get; init; }
        public Task HostTask { get; init; }

        public HostFixture()
        {
            var hostBuilder = new HostBuilder()
                .UseWindowsFormsLifetime<WindowsFormsLifetimeTests.TestForm>()
                .ConfigureServices(services =>
                {
                    services.AddScoped<ScopedDependency>();
                    services.AddSingleton<SingletonDependency>();
                    services.AddTransient<TransientDependency>();
                    services.AddTransient<TestFormWithDependencies>();
                    services.AddTransient<ThrowingForm>(_ => throw new InvalidOperationException("Expected form creation failure."));
                });
            Host = hostBuilder.Build();

            TokenSource = new();
            HostTask = Host.RunAsync(TokenSource.Token);

            Thread.Sleep(2000);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public void Dispose()
        {
            TokenSource.Cancel();
        }
    }

    public abstract class Dependency : IDisposable
    {
        public bool IsDisposed { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public void Dispose() => IsDisposed = true;
    }

    public class ScopedDependency : Dependency
    {   
    }

    public class SingletonDependency : Dependency
    {
    }

    public class TransientDependency : Dependency
    {
    }

    public class TestFormWithDependencies(
        ScopedDependency scopedDependency,
        SingletonDependency singletonDependency,
        TransientDependency transientDependency) : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency ScopedDependency { get; init; } = scopedDependency;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SingletonDependency SingletonDependency { get; init; } = singletonDependency;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TransientDependency TransientDependency { get; init; } = transientDependency;

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

    public class TestFormWithParameters(
        ScopedDependency scopedDependency,
        SingletonDependency singletonDependency,
        TransientDependency transientDependency,
        string parameter1,
        string? parameter2 = null,
        string? parameter3 = null,
        string? parameter4 = null,
        string? parameter5 = null,
        string? parameter6 = null,
        string? parameter7 = null,
        string? parameter8 = null) : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency ScopedDependency { get; init; } = scopedDependency;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SingletonDependency SingletonDependency { get; init; } = singletonDependency;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TransientDependency TransientDependency { get; init; } = transientDependency;

        public string?[] Parameters { get; } = [parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter8];

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

    public class ThrowingForm : Form
    {
    }

    [Fact]
    public void Dependencies_Not_Disposed_Without_A_Scope()
    {
        ScopedDependency? scopedDep = null;
        SingletonDependency? singletonDep = null;
        TransientDependency? transientDep = null;
        using (var form = _host.Host.Services.GetService<TestFormWithDependencies>())
        {
            Assert.NotNull(form);

            scopedDep = form.ScopedDependency;
            Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

            singletonDep = form.SingletonDependency;
            Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

            transientDep = form.TransientDependency;
            Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");
        }

        // Scoped or transient dependencies won't be disposed without a scope.
        Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

        Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

        Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
    }

    [Fact]
    public void Dependencies_Disposed_With_Scope()
    {
        ScopedDependency? scopedDep = null;
        SingletonDependency? singletonDep = null;
        TransientDependency? transientDep = null;
        using (var form = GuiContext.Invoke(() => _host.Host.Services.GetRequiredService<IFormProvider>().GetForm<TestFormWithDependencies>()))
        {
            Assert.NotNull(form);

            scopedDep = form.ScopedDependency;
            Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

            singletonDep = form.SingletonDependency;
            Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

            transientDep = form.TransientDependency;
            Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");
        }

        // Scoped or transient dependencies will be disposed with a scope.
        Assert.True(scopedDep.IsDisposed, "ScopedDependency is not disposed, but should be disposed.");

        Assert.True(transientDep.IsDisposed, "TransientDependency is not disposed, but should be disposed.");

        Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
    }

    [Fact]
    public async Task Dependencies_Disposed_With_Scope_Async()
    {
        ScopedDependency? scopedDep = null;
        SingletonDependency? singletonDep = null;
        TransientDependency? transientDep = null;
        using (var form = await _host.Host.Services.GetRequiredService<IFormProvider>().GetFormAsync<TestFormWithDependencies>())
        {
            Assert.NotNull(form);

            scopedDep = form.ScopedDependency;
            Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

            singletonDep = form.SingletonDependency;
            Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

            transientDep = form.TransientDependency;
            Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");
        }

        // Scoped or transient dependencies will be disposed with a scope.
        Assert.True(scopedDep.IsDisposed, "ScopedDependency is not disposed, but should be disposed.");

        Assert.True(transientDep.IsDisposed, "TransientDependency is not disposed, but should be disposed.");

        Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
    }

    [Fact]
    public void Dependencies_Disposed_With_Shared_Scope()
    {
        ScopedDependency? scopedDep = null;
        SingletonDependency? singletonDep = null;
        TransientDependency? transientDep = null;
        using (var scope = _host.Host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            using (var form = GuiContext.Invoke(() => _host.Host.Services.GetRequiredService<IFormProvider>().GetForm<TestFormWithDependencies>(scope)))
            {
                Assert.NotNull(form);

                scopedDep = form.ScopedDependency;
                Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

                singletonDep = form.SingletonDependency;
                Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

                transientDep = form.TransientDependency;
                Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

                using (var form2 = GuiContext.Invoke(() => _host.Host.Services.GetRequiredService<IFormProvider>().GetForm<TestFormWithDependencies>(scope)))
                {
                    Assert.NotNull(form2);

                    // Separate forms were created
                    Assert.NotSame(form, form2);

                    // Transient dependencies should not the same
                    Assert.NotSame(form.TransientDependency, form2.TransientDependency);

                    // Scoped dependencies should be the same
                    Assert.Same(form.ScopedDependency, form2.ScopedDependency);

                    // Singleton instances should be the same
                    Assert.Same(form.SingletonDependency, form2.SingletonDependency);
                }
            }

            // Dependencies are not disposed because the scope is not disposed.
            Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

            Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

            Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
        } // dispose scope

        // Scoped or transient dependencies will be disposed after the scope is disposed.
        Assert.True(scopedDep.IsDisposed, "ScopedDependency is not disposed, but should be disposed.");

        Assert.True(transientDep.IsDisposed, "TransientDependency is not disposed, but should be disposed.");

        Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
    }

    [Fact]
    public async Task Dependencies_Disposed_With_Shared_Scope_Async()
    {
        ScopedDependency? scopedDep = null;
        SingletonDependency? singletonDep = null;
        TransientDependency? transientDep = null;
        using (var scope = _host.Host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            using (var form = await _host.Host.Services.GetRequiredService<IFormProvider>().GetFormAsync<TestFormWithDependencies>(scope))
            {
                Assert.NotNull(form);

                scopedDep = form.ScopedDependency;
                Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

                singletonDep = form.SingletonDependency;
                Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

                transientDep = form.TransientDependency;
                Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

                using (var form2 = await _host.Host.Services.GetRequiredService<IFormProvider>().GetFormAsync<TestFormWithDependencies>(scope))
                {
                    Assert.NotNull(form2);

                    // Separate forms were created
                    Assert.NotSame(form, form2);

                    // Transient dependencies should not be the same
                    Assert.NotSame(form.TransientDependency, form2.TransientDependency);

                    // Scoped dependencies should be the same
                    Assert.Same(form.ScopedDependency, form2.ScopedDependency);

                    // Singleton instances should be the same
                    Assert.Same(form.SingletonDependency, form2.SingletonDependency);
                }
            }

            // Dependencies are not disposed because the scope is not disposed.
            Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

            Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

            Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
        } // dispose scope

        // Scoped or transient dependencies will be disposed after the scope is disposed.
        Assert.True(scopedDep.IsDisposed, "ScopedDependency is not disposed, but should be disposed.");

        Assert.True(transientDep.IsDisposed, "TransientDependency is not disposed, but should be disposed.");

        Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");
    }

    [Fact]
    public void GetForm_With_One_To_Eight_Parameters_Constructs_And_Disposes_Dependencies()
    {
        var provider = _host.Host.Services.GetRequiredService<IFormProvider>();

        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string>(ParameterValues[0])),
            GetExpectedParameters(1));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string>(ParameterValues[0], ParameterValues[1])),
            GetExpectedParameters(2));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2])),
            GetExpectedParameters(3));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3])),
            GetExpectedParameters(4));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4])),
            GetExpectedParameters(5));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5])),
            GetExpectedParameters(6));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6])),
            GetExpectedParameters(7));
        AssertParameterizedForm(
            GuiContext.Invoke(() => provider.GetForm<TestFormWithParameters, string, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6], ParameterValues[7])),
            GetExpectedParameters(8));
    }

    [Fact]
    public async Task GetFormAsync_With_One_To_Eight_Parameters_Constructs_And_Disposes_Dependencies()
    {
        var provider = _host.Host.Services.GetRequiredService<IFormProvider>();

        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string>(ParameterValues[0]),
            GetExpectedParameters(1));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string>(ParameterValues[0], ParameterValues[1]),
            GetExpectedParameters(2));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2]),
            GetExpectedParameters(3));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3]),
            GetExpectedParameters(4));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4]),
            GetExpectedParameters(5));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5]),
            GetExpectedParameters(6));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6]),
            GetExpectedParameters(7));
        AssertParameterizedForm(
            await provider.GetFormAsync<TestFormWithParameters, string, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6], ParameterValues[7]),
            GetExpectedParameters(8));
    }

    [Fact]
    public async Task GetForm_With_Parameters_Outside_Ui_Thread_Throws()
    {
        var provider = _host.Host.Services.GetRequiredService<IFormProvider>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.Run(() => provider.GetForm<TestFormWithParameters, string>(ParameterValues[0])));
    }

    [Fact]
    public async Task GetFormAsync_Releases_Semaphore_After_Creation_Failure()
    {
        var provider = _host.Host.Services.GetRequiredService<IFormProvider>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetFormAsync<ThrowingForm>());

        var succeedingRequest = provider.GetFormAsync<TestFormWithDependencies>();
        var completedRequest = await Task.WhenAny(succeedingRequest, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(succeedingRequest, completedRequest);

        using var form = await succeedingRequest;
        Assert.NotNull(form);
    }

    private static string[] GetExpectedParameters(int count) => ParameterValues[..count];

    private static void AssertParameterizedForm(TestFormWithParameters form, params string[] expectedParameters)
    {
        using (form)
        {
            for (var index = 0; index < expectedParameters.Length; index++)
            {
                Assert.Equal(expectedParameters[index], form.Parameters[index]);
            }

            for (var index = expectedParameters.Length; index < form.Parameters.Length; index++)
            {
                Assert.Null(form.Parameters[index]);
            }

            Assert.False(form.ScopedDependency.IsDisposed);
            Assert.False(form.SingletonDependency.IsDisposed);
            Assert.False(form.TransientDependency.IsDisposed);
        }

        Assert.True(form.ScopedDependency.IsDisposed);
        Assert.True(form.TransientDependency.IsDisposed);
        Assert.False(form.SingletonDependency.IsDisposed);
    }
}
