using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WindowsFormsLifetime
{
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
		/// <param name="services">The <see cref="IServiceCollection" /> to register the required services to.</param>
		/// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
		/// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
		public static IServiceCollection AddWindowsFormsLifetime<TStartForm>(
			this IServiceCollection services, Action<WindowsFormsLifetimeOptions> configure = null)
			where TStartForm : Form
			=> services
				.AddSingleton<TStartForm>()
				.AddSingleton(provider => new ApplicationContext(provider.GetRequiredService<TStartForm>()))
				.AddWindowsFormsLifetime(configure);

		/// <summary>
		/// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
		/// then waits for the startup context to close before shutting down.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection" /> to register the required services to.</param>
		/// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
		/// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
		/// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
		public static IServiceCollection AddWindowsFormsLifetime<TAppContext>(
			this IServiceCollection services, Func<TAppContext> applicationContextFactory = null, Action<WindowsFormsLifetimeOptions> configure = null)
			where TAppContext : ApplicationContext
		{
			services = applicationContextFactory is null
				? services.AddSingleton<TAppContext>()
				: services.AddSingleton<TAppContext>(provider => applicationContextFactory());

			services.AddSingleton<ApplicationContext, TAppContext>();
			services.AddWindowsFormsLifetime(configure);

			return services;
		}

		/// <summary>
		/// Enables Windows Forms support, builds and starts the host, starts the startup <see cref="ApplicationContext"/>,
		/// then waits for the startup context to close before shutting down.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection" /> to register the required services to.</param>
		/// <param name="applicationContextFactory">The <see cref="ApplicationContext"/> factory.</param>
		/// <param name="configure">The delegate for configuring the <see cref="WindowsFormsLifetimeOptions"/>.</param>
		/// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
		public static IServiceCollection AddWindowsFormsLifetime<TAppContext, TStartForm>(
			this IServiceCollection services, Func<TStartForm, TAppContext> applicationContextFactory, Action<WindowsFormsLifetimeOptions> configure = null)
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
			services.AddWindowsFormsLifetime(configure);

			return services;
		}

}
