using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;
using Xunit;
using WindowsFormsLifetime;
using Microsoft.Extensions.DependencyInjection;

namespace WindowsFormsLifetimeTests
{
    // Put both test classes into the same collection so that their tests are not run in parallel.
    // Otherwise tests fail if both tests run a Host concurrently.
    [Collection("Host tests")]
    public class IFormProviderTests
    {
        public class ScopedDependency : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }

        public class SingletonDependency : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }

        public class TransientDependency : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }

        public class TestFormWithDependencies : Form
        {
            public TestFormWithDependencies(ScopedDependency scopedDependency, SingletonDependency singletonDependency, TransientDependency transientDependency)
            {
                ScopedDependency = scopedDependency;
                SingletonDependency = singletonDependency;
                TransientDependency = transientDependency;
            }

            public ScopedDependency ScopedDependency { get; init; }

            public SingletonDependency SingletonDependency { get; init; }

            public TransientDependency TransientDependency { get; init; }

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

        [Fact]
        public void Dependencies_Not_Disposed_Without_A_Scope()
        {
            var hostBuilder = new HostBuilder()
                                  .UseWindowsFormsLifetime<WindowsFormsLifetimeTests.TestForm>()
                                  .ConfigureServices(services =>
                                  {
                                      services.AddScoped<ScopedDependency>();
                                      services.AddSingleton<SingletonDependency>();
                                      services.AddTransient<TransientDependency>();
                                      services.AddTransient<TestFormWithDependencies>();
                                  });

            using var host = hostBuilder.Build();

            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var form = host.Services.GetService<TestFormWithDependencies>())
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
            var hostBuilder = new HostBuilder()
                                  .UseWindowsFormsLifetime<WindowsFormsLifetimeTests.TestForm>()
                                  .ConfigureServices(services =>
                                  {
                                      services.AddScoped<ScopedDependency>();
                                      services.AddSingleton<SingletonDependency>();
                                      services.AddTransient<TransientDependency>();
                                      services.AddTransient<TestFormWithDependencies>();
                                  });

            using var host = hostBuilder.Build();

            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var form = host.Services.GetRequiredService<IFormProvider>().GetScopedForm<TestFormWithDependencies>())
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
        public async Task Dependencies_Not_Disposed_Without_A_Scope_Async()
        {
            var hostBuilder = new HostBuilder()
                                  .UseWindowsFormsLifetime<WindowsFormsLifetimeTests.TestForm>()
                                  .ConfigureServices(services =>
                                  {
                                      services.AddScoped<ScopedDependency>();
                                      services.AddSingleton<SingletonDependency>();
                                      services.AddTransient<TransientDependency>();
                                      services.AddTransient<TestFormWithDependencies>();
                                  });

            using var host = hostBuilder.Build();

            CancellationTokenSource tokenSource = new();

            Task hostTask = host.RunAsync(tokenSource.Token);

            // Somewhat hacky, wait for the UI thread to start
            await Task.Delay(2000);

            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var form = await host.Services.GetRequiredService<IFormProvider>().GetFormAsync<TestFormWithDependencies>())
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

            tokenSource.Cancel();

            await hostTask;
        }

        [Fact]
        public async Task Dependencies_Disposed_With_Scope_Async()
        {
            var hostBuilder = new HostBuilder()
                                  .UseWindowsFormsLifetime<WindowsFormsLifetimeTests.TestForm>()
                                  .ConfigureServices(services =>
                                  {
                                      services.AddScoped<ScopedDependency>();
                                      services.AddSingleton<SingletonDependency>();
                                      services.AddTransient<TransientDependency>();
                                      services.AddTransient<TestFormWithDependencies>();
                                  });

            using var host = hostBuilder.Build();

            CancellationTokenSource tokenSource = new();

            Task hostTask = host.RunAsync(tokenSource.Token);

            // Somewhat hacky, wait for the UI thread to start
            await Task.Delay(2000);

            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var form = await host.Services.GetRequiredService<IFormProvider>().GetScopedFormAsync<TestFormWithDependencies>())
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

            tokenSource.Cancel();

            await hostTask;
        }
    }
}
