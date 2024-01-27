using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace WindowsFormsLifetime;

public static class ServiceCollectionExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
    public static IServiceCollection AddWindowsFormsLifetime(this IServiceCollection services, Action<WindowsFormsLifetimeOptions> configure, Action<IServiceProvider> preApplicationRunAction = null)
    {
        services.AddSingleton<IHostLifetime, WindowsFormsLifetime>();
        services.AddHostedService(sp =>
        {
            var options = sp.GetRequiredService<IOptions<WindowsFormsLifetimeOptions>>();
            var life = sp.GetRequiredService<IHostApplicationLifetime>();
            var sync = sp.GetRequiredService<WindowsFormsSynchronizationContextProvider>();
            return new WindowsFormsHostedService(options, life, sp, sync, preApplicationRunAction);
        });
        services.Configure(configure ?? (_ => new WindowsFormsLifetimeOptions()));

        services.AddSingleton<IFormProvider, FormProvider>();

        // Synchronization context
        services.AddSingleton<WindowsFormsSynchronizationContextProvider>();
        services.AddSingleton<IWindowsFormsSynchronizationContextProvider>(sp => sp.GetRequiredService<WindowsFormsSynchronizationContextProvider>());
        services.AddSingleton<IGuiContext>(sp => sp.GetRequiredService<WindowsFormsSynchronizationContextProvider>());

        return services;
    }

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="Form"/>,
    /// then waits for the startup form to close before shutting down.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the required services to.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <param name="preApplicationRunAction">The delegate to execute before the application starts running.</param>
    /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWindowsFormsLifetime<TStartForm>(
        this IServiceCollection services, Action<WindowsFormsLifetimeOptions> configure = null, Action<IServiceProvider> preApplicationRunAction = null)
        where TStartForm : Form
        => services
            .AddSingleton<TStartForm>()
            .AddSingleton(provider => new ApplicationContext(provider.GetRequiredService<TStartForm>()))
            .AddWindowsFormsLifetime(configure, preApplicationRunAction);

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the required services to.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <param name="preApplicationRunAction">The delegate to execute before the application starts running.</param>
    /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWindowsFormsLifetime<TAppContext>(
        this IServiceCollection services, Func<TAppContext> applicationContextFactory = null, Action<WindowsFormsLifetimeOptions> configure = null, Action<IServiceProvider> preApplicationRunAction = null)
        where TAppContext : ApplicationContext
    {
        services = applicationContextFactory is null
            ? services.AddSingleton<TAppContext>()
            : services.AddSingleton<TAppContext>(provider => applicationContextFactory());

        services.AddSingleton<ApplicationContext, TAppContext>();
        services.AddWindowsFormsLifetime(configure, preApplicationRunAction);

        return services;
    }

    /// <summary>
    /// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
    /// then waits for the startup context to close before shutting down.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the required services to.</param>
    /// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
    /// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
    /// <param name="preApplicationRunAction">The delegate to execute before the application starts running.</param>
    /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWindowsFormsLifetime<TAppContext, TStartForm>(
        this IServiceCollection services, Func<TStartForm, TAppContext> applicationContextFactory, Action<WindowsFormsLifetimeOptions> configure = null, Action<IServiceProvider> preApplicationRunAction = null)
        where TAppContext : ApplicationContext
        where TStartForm : Form
    {
        services.AddSingleton<TStartForm>();
        services.AddSingleton<TAppContext>(provider =>
        {
            var startForm = provider.GetRequiredService<TStartForm>();
            return applicationContextFactory(startForm);
        });
        services.AddSingleton<ApplicationContext, TAppContext>();
        services.AddWindowsFormsLifetime(configure, preApplicationRunAction);

        return services;
    }

    /// <inheritdoc cref="AddFormsFromAssembliesContainingTypes(IServiceCollection, IEnumerable{Type})"/>
    public static IServiceCollection AddFormsFromAssembliesContainingTypes(this IServiceCollection services, params Type[] markerTypes)
        => services.AddFormsFromAssembliesContainingTypes(markerTypes.AsEnumerable());

    /// <summary>
    /// Scans the <see cref="Assembly"/> containing the given <paramref name="markerTypes"/>
    /// and tries to add all <see cref="Type"/>s that derive from <see cref="Form"/> to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the <see cref="Form"/>s to.</param>
    /// <param name="markerTypes">The <see cref="Type"/>s whose <see cref="Assembly"/>s should be scanned.</param>
    /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFormsFromAssembliesContainingTypes(this IServiceCollection services, IEnumerable<Type> markerTypes)
    {
        foreach (var markerType in markerTypes)
        {
            services.AddFormsFromAssemblyContainingType(markerType);
        }

        return services;
    }

    /// <summary>
    /// Scans the <see cref="Assembly"/> containing <typeparamref name="TMarker"/>
    /// and tries to add all <see cref="Type"/>s that derive from <see cref="Form"/> to <paramref name="services"/>.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="Type"/> whose <see cref="Assembly"/> should be scanned.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the <see cref="Form"/>s to.</param>
    /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFormsFromAssemblyContainingType<TMarker>(this IServiceCollection services)
        => services.AddFormsFromAssemblyContainingType(typeof(TMarker));

    /// <summary>
    /// Scans the <see cref="Assembly"/> containing the given <paramref name="markerType"/>
    /// and tries to add all <see cref="Type"/>s that derive from <see cref="Form"/> to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the <see cref="Form"/>s to.</param>
    /// <param name="markerType">The <see cref="Type"/> whose <see cref="Assembly"/> should be scanned.</param>
    /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFormsFromAssemblyContainingType(this IServiceCollection services, Type markerType)
    {
        var formTypes = markerType.Assembly
            .DefinedTypes
            .Where(t => t.IsAssignableTo(typeof(Form)));

        foreach (var formType in formTypes)
        {
            services.TryAddTransient(formType);
        }

        return services;
    }
}
