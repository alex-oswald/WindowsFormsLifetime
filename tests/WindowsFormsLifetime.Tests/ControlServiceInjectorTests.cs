using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using WindowsFormsLifetime;
using Xunit;

namespace WindowsFormsLifetimeTests;

[Collection("Host tests")]
public class ControlServiceInjectorTests(ControlServiceInjectorTests.HostFixture host) : IClassFixture<ControlServiceInjectorTests.HostFixture>
{
    private readonly HostFixture _host = host;
    private static readonly string[] ParameterValues = ["one", "two", "three", "four", "five", "six", "seven", "eight"];

    private IControlServiceInjector Injector => _host.Host.Services.GetRequiredService<IControlServiceInjector>();
    private IFormProvider FormProvider => _host.Host.Services.GetRequiredService<IFormProvider>();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Registration_Is_Order_Independent_And_Idempotent(bool injectionFirst)
    {
        ServiceCollection services = new();
        if (injectionFirst)
        {
            Assert.Same(services, services.AddWindowsFormsControlInjection());
        }

        services.AddWindowsFormsLifetime<StartupForm>();
        services.AddWindowsFormsControlInjection();
        services.AddWindowsFormsControlInjection();

        ServiceDescriptor descriptor = Assert.Single(services, item => item.ServiceType == typeof(IControlServiceInjector));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        Assert.Same(provider.GetRequiredService<IControlServiceInjector>(), scope.ServiceProvider.GetRequiredService<IControlServiceInjector>());
    }

    [Fact]
    public void Registration_Preserves_An_Existing_Injector()
    {
        ServiceCollection services = new();
        RecordingInjector injector = new();
        services.AddSingleton<IControlServiceInjector>(injector);
        services.AddWindowsFormsControlInjection();

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Same(injector, provider.GetRequiredService<IControlServiceInjector>());
    }

    [Fact]
    public void Registration_Rejects_A_Null_ServiceCollection()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddWindowsFormsControlInjection(null!));
    }

    [Fact]
    public Task Inject_Uses_Existing_Root_Nested_And_Inherited_Controls() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using InheritedControl root = new();
        Panel panel = new();
        TabControl tabs = new();
        TabPage page = new();
        ServiceControl nested = new();
        ServiceControl direct = new();
        object unmarked = new();
        root.Unmarked = unmarked;
        root.Controls.Add(direct);
        root.Controls.Add(panel);
        panel.Controls.Add(tabs);
        tabs.TabPages.Add(page);
        page.Controls.Add(nested);

        Assert.Null(root.Service);
        Assert.Null(direct.Service);
        Assert.Null(nested.Service);
        Injector.Inject(root, scope.ServiceProvider);

        Assert.Same(root, direct.Parent);
        Assert.Same(page, nested.Parent);
        Assert.Same(scope.ServiceProvider.GetRequiredService<ScopedDependency>(), root.Service);
        Assert.Same(root.Service, direct.Service);
        Assert.Same(root.Service, nested.Service);
        Assert.Same(unmarked, root.Unmarked);
        Assert.Equal(1, root.CallbackCount);
        Assert.Equal(1, direct.CallbackCount);
        Assert.Equal(1, nested.CallbackCount);
        Assert.Same(SynchronizationContext.Current, root.CallbackContext);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    });

    [Fact]
    public Task Inject_Assigns_The_Whole_Tree_Before_Child_First_Callbacks() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using ServiceControl root = new();
        ServiceControl child = new();
        ServiceControl sibling = new();
        CallbackControl callbackOnly = new();
        root.Controls.Add(child);
        root.Controls.Add(sibling);
        child.Controls.Add(callbackOnly);
        List<string> callbacks = [];
        Action assertAllAssigned = () =>
        {
            Assert.NotNull(root.Service);
            Assert.NotNull(child.Service);
            Assert.NotNull(sibling.Service);
        };
        callbackOnly.Callback = () =>
        {
            assertAllAssigned();
            callbacks.Add("callback-only");
        };
        child.Callback = () =>
        {
            assertAllAssigned();
            Assert.Equal(1, callbackOnly.CallbackCount);
            callbacks.Add("child");
        };
        sibling.Callback = () =>
        {
            assertAllAssigned();
            callbacks.Add("sibling");
        };
        root.Callback = () =>
        {
            assertAllAssigned();
            Assert.Equal(1, child.CallbackCount);
            Assert.Equal(1, sibling.CallbackCount);
            callbacks.Add("root");
        };

        Injector.Inject(root, scope.ServiceProvider);

        Assert.Equal(4, callbacks.Count);
        Assert.True(callbacks.IndexOf("callback-only") < callbacks.IndexOf("child"));
        Assert.True(callbacks.IndexOf("child") < callbacks.IndexOf("root"));
        Assert.True(callbacks.IndexOf("sibling") < callbacks.IndexOf("root"));
    });

    [Fact]
    public Task Inject_Only_Initializes_UserControls_Not_Other_Containers() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using NonUserControl root = new();
        ServiceControl child = new();
        root.Controls.Add(child);

        Injector.Inject(root, scope.ServiceProvider);

        Assert.Null(root.Service);
        Assert.Equal(0, root.CallbackCount);
        Assert.NotNull(child.Service);
        Assert.Equal(1, child.CallbackCount);
    });

    [Fact]
    public Task Inject_Is_Idempotent_And_Initializes_Only_New_Descendants() => _host.OnUiAsync(() =>
    {
        int resolutions = 0;
        ServiceCollection firstServices = new();
        firstServices.AddTransient<TransientDependency>(_ =>
        {
            resolutions++;
            return new TransientDependency();
        });
        using ServiceProvider firstProvider = firstServices.BuildServiceProvider();
        ServiceCollection secondServices = new();
        secondServices.AddTransient<TransientDependency>();
        using ServiceProvider secondProvider = secondServices.BuildServiceProvider();
        using TransientControl root = new();

        Injector.Inject(root, firstProvider);
        TransientDependency original = Assert.IsType<TransientDependency>(root.Service);
        Injector.Inject(root, firstProvider);
        Injector.Inject(root, secondProvider);

        Assert.Equal(1, resolutions);
        Assert.Same(original, root.Service);
        Assert.Equal(1, root.CallbackCount);

        TransientControl child = new();
        root.Controls.Add(child);
        Assert.Null(child.Service);
        Injector.Inject(root, secondProvider);

        Assert.Same(original, root.Service);
        Assert.NotSame(original, child.Service);
        Assert.Equal(1, root.CallbackCount);
        Assert.Equal(1, child.CallbackCount);
    });

    [Fact]
    public Task Inject_Does_Not_Discover_Controls_Added_By_A_Callback_Until_Explicitly_Requested() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using ServiceControl root = new();
        ServiceControl later = new();
        root.Callback = () => root.Controls.Add(later);

        Injector.Inject(root, scope.ServiceProvider);
        Assert.Null(later.Service);
        Assert.Equal(0, later.CallbackCount);

        Injector.Inject(later, scope.ServiceProvider);
        Assert.Same(root.Service, later.Service);
        Assert.Equal(1, later.CallbackCount);
        Assert.Equal(1, root.CallbackCount);
    });

    [Fact]
    public Task Missing_Service_Leaves_The_Entire_Tree_Unassigned_And_Reports_The_Property() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using ServiceControl root = new();
        ServiceControl validChild = new();
        MissingServiceControl child = new();
        root.Controls.Add(validChild);
        root.Controls.Add(child);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));

        Assert.Contains(nameof(MissingServiceControl), exception.Message);
        Assert.Contains(nameof(MissingServiceControl.Service), exception.Message);
        Assert.Contains(nameof(MissingDependency), exception.ToString());
        Assert.NotNull(exception.InnerException);
        Assert.Null(root.Service);
        Assert.Null(validChild.Service);
        Assert.Null(child.Service);
        Assert.Equal(0, root.CallbackCount);
        Assert.Equal(0, validChild.CallbackCount);
        Assert.Equal(0, child.CallbackCount);
    });

    [Fact]
    public Task Resolution_Can_Be_Retried_When_No_Properties_Or_Callbacks_Have_Run() => _host.OnUiAsync(() =>
    {
        ServiceCollection missingServices = new();
        missingServices.AddTransient<ScopedDependency>();
        using ServiceProvider missingProvider = missingServices.BuildServiceProvider();
        ServiceCollection completeServices = new();
        completeServices.AddTransient<ScopedDependency>();
        completeServices.AddTransient<MissingDependency>();
        using ServiceProvider completeProvider = completeServices.BuildServiceProvider();
        using ServiceControl root = new();
        MissingServiceControl child = new();
        root.Controls.Add(child);

        Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, missingProvider));
        Assert.Null(root.Service);
        Assert.Equal(0, root.CallbackCount);
        Injector.Inject(root, completeProvider);

        Assert.NotNull(root.Service);
        Assert.NotNull(child.Service);
        Assert.Equal(1, root.CallbackCount);
        Assert.Equal(1, child.CallbackCount);
    });

    [Fact]
    public Task Resolution_Failure_Preserves_The_Original_Exception() => _host.OnUiAsync(() =>
    {
        InvalidOperationException expected = new("Service factory failed.");
        ServiceCollection services = new();
        services.AddTransient<ScopedDependency>(_ => throw expected);
        using ServiceProvider provider = services.BuildServiceProvider();
        using ServiceControl root = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, provider));

        Assert.Contains(nameof(ServiceControl), exception.Message);
        Assert.Contains(nameof(ServiceControl.Service), exception.Message);
        Assert.Same(expected, exception.GetBaseException());
        Assert.Equal(0, root.CallbackCount);
    });

    [Theory]
    [InlineData(typeof(PrivateSetterControl), "Service")]
    [InlineData(typeof(ProtectedSetterControl), "Service")]
    [InlineData(typeof(PrivatePropertyControl), "Service")]
    [InlineData(typeof(StaticPropertyControl), "Service")]
    [InlineData(typeof(IndexerControl), "Item")]
    [InlineData(typeof(ReadOnlyControl), "Service")]
    [InlineData(typeof(InitOnlyControl), "Service")]
    public Task Invalid_Property_Shapes_Are_Rejected_Before_Resolution(Type controlType, string propertyName) => _host.OnUiAsync(() =>
    {
        int resolutions = 0;
        ServiceCollection services = new();
        services.AddTransient<ScopedDependency>(_ =>
        {
            resolutions++;
            return new ScopedDependency();
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using ServiceControl root = new();
        ServiceControl validChild = new();
        Control invalid = Assert.IsAssignableFrom<Control>(Activator.CreateInstance(controlType));
        root.Controls.Add(validChild);
        root.Controls.Add(invalid);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, provider));

        Assert.Contains(controlType.Name, exception.Message);
        Assert.Contains(propertyName, exception.Message);
        Assert.Equal(0, resolutions);
        Assert.Null(root.Service);
        Assert.Null(validChild.Service);
        Assert.Equal(0, root.CallbackCount);
        Assert.Equal(0, validChild.CallbackCount);
    });

    [Fact]
    public Task Throwing_Setter_Is_Not_Retried_And_Prevents_All_Callbacks() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using ServiceControl root = new();
        ThrowingSetterControl child = new();
        root.Controls.Add(child);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));

        Assert.Contains(nameof(ThrowingSetterControl), exception.Message);
        Assert.Contains(nameof(ThrowingSetterControl.Service), exception.Message);
        Assert.Same(child.Failure, exception.GetBaseException());
        Assert.Equal(1, child.SetterCount);
        Assert.Equal(0, root.CallbackCount);
        Assert.Equal(0, child.CallbackCount);
        Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));
        Assert.Equal(1, child.SetterCount);
        Assert.Equal(0, root.CallbackCount);
        Assert.False(root.IsDisposed);
        Assert.False(child.IsDisposed);
    });

    [Fact]
    public Task Throwing_Callback_Is_Not_Retried_And_Prevents_Parent_Callback() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using ServiceControl root = new();
        ServiceControl child = new();
        InvalidOperationException expected = new("Callback failed.");
        child.Callback = () => throw expected;
        root.Controls.Add(child);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));

        Assert.Contains(nameof(ServiceControl), exception.Message);
        Assert.Same(expected, exception.GetBaseException());
        Assert.NotNull(root.Service);
        Assert.NotNull(child.Service);
        Assert.Equal(1, child.CallbackCount);
        Assert.Equal(0, root.CallbackCount);
        Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));
        Assert.Equal(1, child.CallbackCount);
        Assert.Equal(0, root.CallbackCount);
    });

    [Fact]
    public Task Recursive_Initialization_Is_Rejected_Without_Repeating_Callbacks() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using ServiceControl root = new();
        root.Callback = () => Injector.Inject(root, scope.ServiceProvider);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));

        Assert.Contains(nameof(ServiceControl), exception.ToString());
        Assert.Equal(1, root.CallbackCount);
        Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, scope.ServiceProvider));
        Assert.Equal(1, root.CallbackCount);
    });

    [Fact]
    public Task Null_Arguments_And_Disposed_Root_Are_Rejected() => _host.OnUiAsync(() =>
    {
        using ServiceControl root = new();
        Assert.Throws<ArgumentNullException>(() => Injector.Inject(null!, _host.Host.Services));
        Assert.Throws<ArgumentNullException>(() => Injector.Inject(root, null!));
        root.Dispose();

        Assert.Throws<ObjectDisposedException>(() => Injector.Inject(root, _host.Host.Services));
    });

    [Fact]
    public async Task Wrong_Thread_Is_Rejected_Before_Initialization()
    {
        ServiceControl root = await _host.OnUiAsync(() => new ServiceControl());
        try
        {
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => Task.Run(() => Injector.Inject(root, _host.Host.Services)));
            Assert.Contains("UI", exception.Message);
            Assert.Null(root.Service);
            Assert.Equal(0, root.CallbackCount);
        }
        finally
        {
            await _host.OnUiAsync(root.Dispose);
        }
    }

    [Fact]
    public Task Unavailable_Context_Is_Reported_Clearly() => _host.OnUiAsync(() =>
    {
        ServiceCollection services = new();
        services.AddSingleton<IWindowsFormsSynchronizationContextProvider>(new UnavailableContextProvider());
        services.AddWindowsFormsControlInjection();
        using ServiceProvider provider = services.BuildServiceProvider();
        using ServiceControl root = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => provider.GetRequiredService<IControlServiceInjector>().Inject(root, provider));

        Assert.Contains("UI", exception.Message);
        Assert.Equal(0, root.CallbackCount);
    });

    [Fact]
    public Task Root_Scope_Validation_Is_Not_Bypassed() => _host.OnUiAsync(() =>
    {
        using ServiceControl root = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Injector.Inject(root, _host.Host.Services));

        Assert.Contains(nameof(ScopedDependency), exception.ToString());
        Assert.Contains("root provider", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(root.Service);
        Assert.Equal(0, root.CallbackCount);
    });

    [Fact]
    public Task Injector_Does_Not_Own_External_Controls_Or_Their_Services() => _host.OnUiAsync(() =>
    {
        ServiceCollection services = new();
        services.AddScoped<ScopedDependency>();
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using IServiceScope scope = provider.CreateScope();
        ServiceControl root = new();
        Injector.Inject(root, scope.ServiceProvider);
        ScopedDependency service = Assert.IsType<ScopedDependency>(root.Service);

        root.Dispose();
        Assert.False(service.IsDisposed);
        Assert.Same(service, scope.ServiceProvider.GetRequiredService<ScopedDependency>());
        scope.Dispose();
        Assert.True(service.IsDisposed);
    });

    [Fact]
    public async Task Weak_State_Does_Not_Keep_A_Control_Or_Provider_Alive()
    {
        (WeakReference Control, WeakReference Provider) references = await _host.OnUiAsync(() => CreateWeakReferences(Injector));
        await _host.OnUiAsync(() => true);

        for (int attempt = 0; attempt < 5 && (references.Control.IsAlive || references.Provider.IsAlive); attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.False(references.Control.IsAlive);
        Assert.False(references.Provider.IsAlive);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Registered_Forms_Are_Injected_Using_Their_Owned_Scope(bool asynchronous)
    {
        InjectedForm form = asynchronous
            ? await FormProvider.GetFormAsync<InjectedForm>().WaitAsync(HostFixture.Timeout)
            : await _host.OnUiAsync(() => FormProvider.GetForm<InjectedForm>());
        await _host.OnUiAsync(() => AssertInjectedFormAndDispose(form));
    }

    [Fact]
    public Task Raw_DI_Resolution_Does_Not_Initialize_A_Form_Until_The_Helper_Is_Called() => _host.OnUiAsync(() =>
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        using InjectedForm form = scope.ServiceProvider.GetRequiredService<InjectedForm>();
        Assert.Null(form.Child.Service);
        Assert.Equal(0, form.Child.CallbackCount);

        Injector.Inject(form, scope.ServiceProvider);

        Assert.Same(form.ScopedDependency, form.Child.Service);
        Assert.Equal(1, form.Child.CallbackCount);
    });

    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(false, 4)]
    [InlineData(false, 5)]
    [InlineData(false, 6)]
    [InlineData(false, 7)]
    [InlineData(false, 8)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    [InlineData(true, 4)]
    [InlineData(true, 5)]
    [InlineData(true, 6)]
    [InlineData(true, 7)]
    [InlineData(true, 8)]
    public async Task All_Parameter_Overloads_Inject_And_Dispose_Their_Owned_Scope(bool asynchronous, int count)
    {
        ParameterForm form = asynchronous
            ? await GetParameterFormAsync(count).WaitAsync(HostFixture.Timeout)
            : await _host.OnUiAsync(() => GetParameterForm(count));
        await _host.OnUiAsync(() =>
        {
            try
            {
                Assert.Equal(ParameterValues.Take(count), form.Parameters.Take(count));
                Assert.All(form.Parameters.Skip(count), Assert.Null);
            }
            finally
            {
                AssertInjectedFormAndDispose(form);
            }
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Caller_Scope_Overloads_Share_Services_And_Do_Not_Take_Ownership(bool asynchronous)
    {
        using IServiceScope scope = _host.Host.Services.CreateScope();
        InjectedForm first = asynchronous
            ? await FormProvider.GetFormAsync<InjectedForm>(scope).WaitAsync(HostFixture.Timeout)
            : await _host.OnUiAsync(() => FormProvider.GetForm<InjectedForm>(scope));
        InjectedForm second = asynchronous
            ? await FormProvider.GetFormAsync<InjectedForm>(scope).WaitAsync(HostFixture.Timeout)
            : await _host.OnUiAsync(() => FormProvider.GetForm<InjectedForm>(scope));
        await _host.OnUiAsync(() =>
        {
            ScopedDependency scoped = first.ScopedDependency;
            TransientDependency transient = Assert.IsType<TransientDependency>(first.Child.TransientService);
            try
            {
                Assert.NotSame(first, second);
                Assert.Same(scoped, first.Child.Service);
                Assert.Same(scoped, second.Child.Service);
                Assert.NotSame(first.Child.TransientService, second.Child.TransientService);
                Assert.Equal(1, first.Child.CallbackCount);
                Assert.Equal(1, second.Child.CallbackCount);
            }
            finally
            {
                first.Dispose();
                second.Dispose();
            }

            Assert.False(scoped.IsDisposed);
            Assert.False(transient.IsDisposed);
            Assert.Same(scoped, scope.ServiceProvider.GetRequiredService<ScopedDependency>());
            scope.Dispose();
            Assert.True(scoped.IsDisposed);
            Assert.True(transient.IsDisposed);
            Assert.False(first.Child.SingletonService!.IsDisposed);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_Registered_Forms_Release_Their_Owned_Scope_And_Semaphore(bool asynchronous)
    {
        ServiceCollection services = CreateFormServices();
        FailureRecord record = new();
        services.AddSingleton(record);
        services.AddTransient<FailureForm>();
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        IFormProvider forms = provider.GetRequiredService<IFormProvider>();

        if (asynchronous)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => forms.GetFormAsync<FailureForm>().WaitAsync(HostFixture.Timeout));
        }
        else
        {
            await _host.OnUiAsync(() => Assert.Throws<InvalidOperationException>(() => forms.GetForm<FailureForm>()));
        }

        await _host.OnUiAsync(() =>
        {
            Assert.NotNull(record.Form);
            Assert.True(record.Form.IsDisposed);
            Assert.True(record.Dependency!.IsDisposed);
        });
        InjectedForm succeeding = await forms.GetFormAsync<InjectedForm>().WaitAsync(HostFixture.Timeout);
        await _host.OnUiAsync(() => AssertInjectedFormAndDispose(succeeding));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_Parameter_Forms_Are_Explicitly_Disposed(bool asynchronous)
    {
        ServiceCollection services = CreateFormServices();
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        IFormProvider forms = provider.GetRequiredService<IFormProvider>();
        FailureRecord record = new();

        if (asynchronous)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => forms.GetFormAsync<FailedParameterForm, FailureRecord>(record).WaitAsync(HostFixture.Timeout));
        }
        else
        {
            await _host.OnUiAsync(() => Assert.Throws<InvalidOperationException>(() => forms.GetForm<FailedParameterForm, FailureRecord>(record)));
        }

        await _host.OnUiAsync(() =>
        {
            Assert.NotNull(record.Form);
            Assert.True(record.Form.IsDisposed);
            Assert.True(record.Child!.IsDisposed);
            Assert.True(record.Dependency!.IsDisposed);
            Assert.Equal(1, record.DisposeCount);
        });
        InjectedForm succeeding = await forms.GetFormAsync<InjectedForm>().WaitAsync(HostFixture.Timeout);
        await _host.OnUiAsync(() => AssertInjectedFormAndDispose(succeeding));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_Caller_Scope_Forms_Leave_The_Scope_And_Form_Owned_By_The_Caller(bool asynchronous)
    {
        ServiceCollection services = CreateFormServices();
        FailureRecord record = new();
        services.AddSingleton(record);
        services.AddScoped<FailureForm>();
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using IServiceScope scope = provider.CreateScope();
        IFormProvider forms = provider.GetRequiredService<IFormProvider>();

        if (asynchronous)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => forms.GetFormAsync<FailureForm>(scope).WaitAsync(HostFixture.Timeout));
        }
        else
        {
            await _host.OnUiAsync(() => Assert.Throws<InvalidOperationException>(() => forms.GetForm<FailureForm>(scope)));
        }

        await _host.OnUiAsync(() =>
        {
            Assert.False(record.Form!.IsDisposed);
            Assert.False(record.Dependency!.IsDisposed);
            Assert.Same(record.Dependency, scope.ServiceProvider.GetRequiredService<ScopedDependency>());
            Assert.Same(record.Form, scope.ServiceProvider.GetRequiredService<FailureForm>());
            scope.Dispose();
            Assert.True(record.Form.IsDisposed);
            Assert.True(record.Dependency.IsDisposed);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_Registered_External_Forms_Are_Not_Disposed(bool asynchronous)
    {
        FailureRecord record = new();
        using ScopedDependency dependency = new();
        FailureForm external = await _host.OnUiAsync(() => new FailureForm(dependency, record));
        try
        {
            ServiceCollection services = CreateFormServices();
            services.AddSingleton(external);
            using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            IFormProvider forms = provider.GetRequiredService<IFormProvider>();
            if (asynchronous)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => forms.GetFormAsync<FailureForm>().WaitAsync(HostFixture.Timeout));
            }
            else
            {
                await _host.OnUiAsync(() => Assert.Throws<InvalidOperationException>(() => forms.GetForm<FailureForm>()));
            }

            await _host.OnUiAsync(() =>
            {
                Assert.False(external.IsDisposed);
                Assert.False(record.Child!.IsDisposed);
                Assert.False(dependency.IsDisposed);
            });
        }
        finally
        {
            await _host.OnUiAsync(external.Dispose);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task Feature_Disabled_Preserves_Ordinary_And_Parameterized_Form_Behavior(bool parameterized) => _host.OnUiAsync(() =>
    {
        ServiceCollection services = CreateFormServices(enableInjection: false);
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        Assert.Null(provider.GetService<IControlServiceInjector>());
        IFormProvider forms = provider.GetRequiredService<IFormProvider>();
        using InjectedForm form = parameterized
            ? forms.GetForm<ParameterForm, string>("not injected")
            : forms.GetForm<InjectedForm>();

        Assert.Null(form.Child.Service);
        Assert.Null(form.Child.SingletonService);
        Assert.Null(form.Child.TransientService);
        Assert.Equal(0, form.Child.CallbackCount);
    });

    [Fact]
    public async Task Unregistered_Forms_Remain_Null_With_Injection_Enabled()
    {
        Assert.Null(await FormProvider.GetFormAsync<UnregisteredForm>().WaitAsync(HostFixture.Timeout));
        await _host.OnUiAsync(() => Assert.Null(FormProvider.GetForm<UnregisteredForm>()));
        using IServiceScope scope = _host.Host.Services.CreateScope();
        Assert.Null(await FormProvider.GetFormAsync<UnregisteredForm>(scope).WaitAsync(HostFixture.Timeout));
        await _host.OnUiAsync(() => Assert.Null(FormProvider.GetForm<UnregisteredForm>(scope)));
    }

    [Fact]
    public async Task Startup_Uses_The_Root_Provider_Before_PreAction_And_Display()
    {
        StartupForm form = Assert.IsType<StartupForm>(await FormProvider.GetMainFormAsync());
        await _host.OnUiAsync(() =>
        {
            Assert.False(form.WasInjectedDuringConstruction);
            Assert.True(_host.WasInjectedBeforePreAction);
            Assert.True(form.WasInjectedBeforeDisplay);
            Assert.Same(_host.Host.Services.GetRequiredService<IServiceProvider>(), form.Child.Provider);
            Assert.Same(_host.Host.Services.GetRequiredService<SingletonDependency>(), form.Child.Service);
            Assert.Equal(1, form.Child.CallbackCount);
        });

        Assert.Same(form, await FormProvider.GetMainFormAsync());
        await _host.OnUiAsync(() => Assert.Equal(1, form.Child.CallbackCount));
    }

    private ServiceCollection CreateFormServices(bool enableInjection = true)
    {
        ServiceCollection services = new();
        services.AddSingleton(_host.Host.Services.GetRequiredService<IWindowsFormsSynchronizationContextProvider>());
        services.AddSingleton<IFormProvider, WindowsFormsLifetime.FormProvider>();
        services.AddScoped<ScopedDependency>();
        services.AddSingleton<SingletonDependency>();
        services.AddTransient<TransientDependency>();
        services.AddTransient<InjectedForm>();
        if (enableInjection)
        {
            services.AddWindowsFormsControlInjection();
        }

        return services;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Control, WeakReference Provider) CreateWeakReferences(IControlServiceInjector injector)
    {
        ServiceCollection services = new();
        services.AddTransient<TransientDependency>();
        using ServiceProvider provider = services.BuildServiceProvider();
        using TransientControl control = new();
        injector.Inject(control, provider);
        return (new WeakReference(control), new WeakReference(provider));
    }

    private static void AssertInjectedFormAndDispose(InjectedForm form)
    {
        ScopedDependency? scoped = form.Child.Service;
        TransientDependency? transient = form.Child.TransientService;
        SingletonDependency? singleton = form.Child.SingletonService;
        try
        {
            Assert.False(form.WasInjectedDuringConstruction);
            Assert.Same(form.ScopedDependency, scoped);
            Assert.NotNull(transient);
            Assert.NotNull(singleton);
            Assert.False(scoped!.IsDisposed);
            Assert.False(transient.IsDisposed);
            Assert.False(singleton.IsDisposed);
            Assert.Equal(1, form.Child.CallbackCount);
            Assert.Same(SynchronizationContext.Current, form.Child.CallbackContext);
        }
        finally
        {
            form.Dispose();
        }

        Assert.True(form.Child.IsDisposed);
        Assert.True(scoped!.IsDisposed);
        Assert.True(transient!.IsDisposed);
        Assert.False(singleton!.IsDisposed);
    }

    private ParameterForm GetParameterForm(int count) => count switch
    {
        1 => FormProvider.GetForm<ParameterForm, string>(ParameterValues[0]),
        2 => FormProvider.GetForm<ParameterForm, string, string>(ParameterValues[0], ParameterValues[1]),
        3 => FormProvider.GetForm<ParameterForm, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2]),
        4 => FormProvider.GetForm<ParameterForm, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3]),
        5 => FormProvider.GetForm<ParameterForm, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4]),
        6 => FormProvider.GetForm<ParameterForm, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5]),
        7 => FormProvider.GetForm<ParameterForm, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6]),
        8 => FormProvider.GetForm<ParameterForm, string, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6], ParameterValues[7]),
        _ => throw new ArgumentOutOfRangeException(nameof(count))
    };

    private Task<ParameterForm> GetParameterFormAsync(int count) => count switch
    {
        1 => FormProvider.GetFormAsync<ParameterForm, string>(ParameterValues[0]),
        2 => FormProvider.GetFormAsync<ParameterForm, string, string>(ParameterValues[0], ParameterValues[1]),
        3 => FormProvider.GetFormAsync<ParameterForm, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2]),
        4 => FormProvider.GetFormAsync<ParameterForm, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3]),
        5 => FormProvider.GetFormAsync<ParameterForm, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4]),
        6 => FormProvider.GetFormAsync<ParameterForm, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5]),
        7 => FormProvider.GetFormAsync<ParameterForm, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6]),
        8 => FormProvider.GetFormAsync<ParameterForm, string, string, string, string, string, string, string, string>(ParameterValues[0], ParameterValues[1], ParameterValues[2], ParameterValues[3], ParameterValues[4], ParameterValues[5], ParameterValues[6], ParameterValues[7]),
        _ => throw new ArgumentOutOfRangeException(nameof(count))
    };

    public class HostFixture : IAsyncLifetime
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
        private readonly bool _enableInjection;
        private readonly bool _hasMainForm;
        private Thread? _uiThread;

        public HostFixture() : this(true, true)
        {
        }

        internal HostFixture(bool enableInjection, bool hasMainForm)
        {
            _enableInjection = enableInjection;
            _hasMainForm = hasMainForm;
        }

        public IHost Host { get; private set; } = null!;
        public bool WasInjectedBeforePreAction { get; private set; }

        public async Task InitializeAsync()
        {
            TaskCompletionSource<Thread> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Host = new HostBuilder()
                .UseDefaultServiceProvider(options => options.ValidateScopes = true)
                .ConfigureServices(services =>
                {
                    services.AddScoped<ScopedDependency>();
                    services.AddSingleton<SingletonDependency>();
                    services.AddTransient<TransientDependency>();
                    services.AddTransient<InjectedForm>();
                    if (_enableInjection)
                    {
                        services.AddWindowsFormsControlInjection();
                    }

                    Action<IServiceProvider> preAction = provider =>
                    {
                        ApplicationContext context = provider.GetRequiredService<ApplicationContext>();
                        WasInjectedBeforePreAction = context.MainForm is StartupForm form && form.Child.Service != null && form.Child.CallbackCount == 1;
                        SynchronizationContext.Current!.Post(_ => ready.TrySetResult(Thread.CurrentThread), null);
                    };
                    if (_hasMainForm)
                    {
                        services.AddWindowsFormsLifetime<StartupForm>(preApplicationRunAction: preAction);
                    }
                    else
                    {
                        services.AddWindowsFormsLifetime<EmptyContext>(preApplicationRunAction: preAction);
                    }
                })
                .Build();

            await Host.StartAsync().WaitAsync(Timeout);
            _uiThread = await ready.Task.WaitAsync(Timeout);
        }

        public Task OnUiAsync(Action action) => OnUiAsync(() =>
        {
            action();
            return true;
        });

        public Task<T> OnUiAsync<T>(Func<T> action) => Host.Services.GetRequiredService<IGuiContext>().InvokeAsync(action).WaitAsync(Timeout);

        public async Task DisposeAsync()
        {
            try
            {
                if (_uiThread is { IsAlive: true })
                {
                    await OnUiAsync(Application.ExitThread);
                    Assert.True(_uiThread.Join(Timeout), "The Windows Forms UI thread did not exit.");
                }

                await Host.StopAsync().WaitAsync(Timeout);
            }
            finally
            {
                Host.Dispose();
            }
        }
    }

    public class CallbackControl : UserControl, IOnServicesInjected
    {
        public int CallbackCount { get; private set; }
        public SynchronizationContext? CallbackContext { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action? Callback { get; set; }

        public void OnServicesInjected()
        {
            CallbackCount++;
            CallbackContext = SynchronizationContext.Current;
            Callback?.Invoke();
        }
    }

    public class ServiceControl : CallbackControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ScopedDependency? Service { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? Unmarked { get; set; }
    }

    public class InheritedControl : ServiceControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ScopedDependency? Service { get; set; }
    }

    public class TransientControl : CallbackControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TransientDependency? Service { get; set; }
    }

    public class LifetimeControl : ServiceControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SingletonDependency? SingletonService { get; set; }

        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TransientDependency? TransientService { get; set; }
    }

    public class MissingServiceControl : CallbackControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MissingDependency? Service { get; set; }
    }

    public class ThrowingSetterControl : CallbackControl
    {
        public InvalidOperationException Failure { get; } = new("Setter failed.");
        public int SetterCount { get; private set; }

        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency Service
        {
            get => throw new NotSupportedException();
            set
            {
                SetterCount++;
                throw Failure;
            }
        }
    }

    // Intentionally invalid declarations exercise runtime validation independently of the analyzer.
#pragma warning disable WFLDI001
    public class NonUserControl : Panel, IOnServicesInjected
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency? Service { get; set; }

        public int CallbackCount { get; private set; }

        public void OnServicesInjected() => CallbackCount++;
    }

    public class PrivateSetterControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency? Service { get; private set; }
    }

    public class ProtectedSetterControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency? Service { get; protected set; }
    }

    public class PrivatePropertyControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private ScopedDependency? Service { get; set; }
    }

    public class StaticPropertyControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static ScopedDependency? Service { get; set; }
    }

    public class IndexerControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency? this[int index]
        {
            get => null;
            set { }
        }
    }

    public class ReadOnlyControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency? Service => null;
    }

    public class InitOnlyControl : UserControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScopedDependency? Service { get; init; }
    }
#pragma warning restore WFLDI001

    public abstract class DisposableDependency : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public class ScopedDependency : DisposableDependency
    {
    }

    public class SingletonDependency : DisposableDependency
    {
    }

    public class TransientDependency : DisposableDependency
    {
    }

    public class MissingDependency
    {
    }

    public class InjectedForm : Form
    {
        public InjectedForm(ScopedDependency scopedDependency)
        {
            ScopedDependency = scopedDependency;
            Child = new LifetimeControl();
            Controls.Add(Child);
            WasInjectedDuringConstruction = Child.Service != null;
        }

        public ScopedDependency ScopedDependency { get; }
        public LifetimeControl Child { get; }
        public bool WasInjectedDuringConstruction { get; }
    }

    public class ParameterForm(
        ScopedDependency scopedDependency,
        string parameter1,
        string? parameter2 = null,
        string? parameter3 = null,
        string? parameter4 = null,
        string? parameter5 = null,
        string? parameter6 = null,
        string? parameter7 = null,
        string? parameter8 = null) : InjectedForm(scopedDependency)
    {
        public string?[] Parameters { get; } = [parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter8];
    }

    public class FailureRecord
    {
        public Form? Form { get; set; }
        public Control? Child { get; set; }
        public ScopedDependency? Dependency { get; set; }
        public int DisposeCount { get; set; }
    }

    public class FailureForm : Form
    {
        public FailureForm(ScopedDependency dependency, FailureRecord record)
        {
            MissingServiceControl child = new();
            Controls.Add(child);
            record.Form = this;
            record.Child = child;
            record.Dependency = dependency;
        }
    }

    public class FailedParameterForm : FailureForm
    {
        private readonly FailureRecord _record;

        public FailedParameterForm(ScopedDependency dependency, FailureRecord record) : base(dependency, record)
        {
            _record = record;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _record.DisposeCount++;
            }

            base.Dispose(disposing);
        }
    }

    public class UnregisteredForm : Form
    {
    }

    public class StartupControl : CallbackControl
    {
        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SingletonDependency? Service { get; set; }

        [InjectService]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceProvider? Provider { get; set; }
    }

    public class StartupForm : Form
    {
        public StartupForm()
        {
            Child = new StartupControl();
            Controls.Add(Child);
            WasInjectedDuringConstruction = Child.Service != null;
        }

        public StartupControl Child { get; }
        public bool WasInjectedDuringConstruction { get; }
        public bool WasInjectedBeforeDisplay { get; private set; }

        protected override void SetVisibleCore(bool value)
        {
            WasInjectedBeforeDisplay = Child.Service != null && Child.CallbackCount == 1;
            base.SetVisibleCore(false);
            if (!IsHandleCreated)
            {
                CreateHandle();
                OnLoad(EventArgs.Empty);
            }
        }
    }

    public class EmptyContext : ApplicationContext
    {
    }

    private sealed class UnavailableContextProvider : IWindowsFormsSynchronizationContextProvider
    {
        public WindowsFormsSynchronizationContext SynchronizationContext => null!;
    }

    private sealed class RecordingInjector : IControlServiceInjector
    {
        public void Inject(Control root, IServiceProvider services)
        {
        }
    }
}

[Collection("Host tests")]
public class ControlInjectionStartupTests
{
    [Fact]
    public async Task Disabled_Feature_Does_Not_Initialize_The_Startup_Form()
    {
        ControlServiceInjectorTests.HostFixture host = new(enableInjection: false, hasMainForm: true);
        await host.InitializeAsync();
        try
        {
            await host.OnUiAsync(() =>
            {
                ControlServiceInjectorTests.StartupForm form = host.Host.Services.GetRequiredService<ControlServiceInjectorTests.StartupForm>();
                Assert.Null(host.Host.Services.GetService<IControlServiceInjector>());
                Assert.False(host.WasInjectedBeforePreAction);
                Assert.False(form.WasInjectedBeforeDisplay);
                Assert.Null(form.Child.Service);
                Assert.Equal(0, form.Child.CallbackCount);
            });
        }
        finally
        {
            await host.DisposeAsync();
        }
    }

    [Fact]
    public async Task Enabled_Feature_Accepts_An_ApplicationContext_Without_A_MainForm()
    {
        ControlServiceInjectorTests.HostFixture host = new(enableInjection: true, hasMainForm: false);
        await host.InitializeAsync();
        try
        {
            await host.OnUiAsync(() =>
            {
                Assert.NotNull(host.Host.Services.GetRequiredService<IControlServiceInjector>());
                Assert.Null(host.Host.Services.GetRequiredService<ApplicationContext>().MainForm);
                Assert.False(host.WasInjectedBeforePreAction);
            });
            Assert.Null(await host.Host.Services.GetRequiredService<IFormProvider>().GetMainFormAsync());
        }
        finally
        {
            await host.DisposeAsync();
        }
    }
}
