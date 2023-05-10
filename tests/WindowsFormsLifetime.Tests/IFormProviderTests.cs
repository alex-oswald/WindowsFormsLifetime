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
    public class IFormProviderTests : IClassFixture<IFormProviderTests.HostFixture>
    {
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
                                              });
                Host = hostBuilder.Build();

                TokenSource = new();
                HostTask = Host.RunAsync(TokenSource.Token);

                Thread.Sleep(2000);
            }

            public void Dispose()
            {
                TokenSource.Cancel();
            }
        }

        public abstract class Dependency : IDisposable
        {
            public bool IsDisposed { get; private set; }

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

        private readonly HostFixture _host;

        public IFormProviderTests(HostFixture host)
        {
            _host = host;
        }

        [Fact]
        public void Dependencies_Not_Disposed_Without_A_Scope()
        {
            //using IHost host = CreateHost();

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
            //using IHost host = CreateHost();

            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var form = _host.Host.Services.GetRequiredService<IFormProvider>().GetScopedForm<TestFormWithDependencies>())
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
            //using IHost host = CreateHost();

            //CancellationTokenSource tokenSource = new();

            //Task hostTask = host.RunAsync(tokenSource.Token);

            // Somewhat hacky, wait for the UI thread to start
            //await Task.Delay(2000);

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

            // Scoped or transient dependencies won't be disposed without a scope.
            Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

            Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

            Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

            //tokenSource.Cancel();

            //await hostTask;
        }

        [Fact]
        public async Task Dependencies_Disposed_With_Scope_Async()
        {
            //(var host, var tokenSource, var hostTask) = BeginStartHost();

            // Somewhat hacky, wait for the UI thread to start
            //await Task.Delay(2000);

            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var form = await _host.Host.Services.GetRequiredService<IFormProvider>().GetScopedFormAsync<TestFormWithDependencies>())
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

            //tokenSource.Cancel();

            //await hostTask;
        }

        [Fact]
        public void Dependencies_Disposed_With_Shared_Scope()
        {
            ScopedDependency? scopedDep = null;
            SingletonDependency? singletonDep = null;
            TransientDependency? transientDep = null;
            using (var scope = _host.Host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var form = _host.Host.Services.GetRequiredService<IFormProvider>().GetScopedForm<TestFormWithDependencies>(scope))
                {
                    Assert.NotNull(form);

                    scopedDep = form.ScopedDependency;
                    Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

                    singletonDep = form.SingletonDependency;
                    Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

                    transientDep = form.TransientDependency;
                    Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

                    using (var form2 = _host.Host.Services.GetRequiredService<IFormProvider>().GetScopedForm<TestFormWithDependencies>(scope))
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
                using (var form = await _host.Host.Services.GetRequiredService<IFormProvider>().GetScopedFormAsync<TestFormWithDependencies>(scope))
                {
                    Assert.NotNull(form);

                    scopedDep = form.ScopedDependency;
                    Assert.False(scopedDep.IsDisposed, "ScopedDependency is disposed, but should not be disposed.");

                    singletonDep = form.SingletonDependency;
                    Assert.False(singletonDep.IsDisposed, "SingletonDependency is disposed, but should not be disposed.");

                    transientDep = form.TransientDependency;
                    Assert.False(transientDep.IsDisposed, "TransientDependency is disposed, but should not be disposed.");

                    using (var form2 = await _host.Host.Services.GetRequiredService<IFormProvider>().GetScopedFormAsync<TestFormWithDependencies>(scope))
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
    }
}
