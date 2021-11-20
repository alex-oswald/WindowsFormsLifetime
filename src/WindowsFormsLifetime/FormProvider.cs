using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
{
    public interface IFormProvider
    {
        /// <summary>
        /// Gets the requested form type and ensures it is created on the UI thread.
        /// </summary>
        /// <typeparam name="T">The form type to get.</typeparam>
        /// <returns>An instance of the form, asynchronously.</returns>
        Task<T> GetFormAsync<T>() where T : Form;

        Task<Form> GetMainFormAsync();
    }

    public class FormProvider : IFormProvider
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationContext _applicationContext;
        private readonly IWindowsFormsSynchronizationContextProvider _syncContextManager;

        public FormProvider(
            IServiceProvider serviceProvider,
            ApplicationContext applicationContext,
            IWindowsFormsSynchronizationContextProvider syncContextManager)
        {
            _serviceProvider = serviceProvider;
            _applicationContext = applicationContext;
            _syncContextManager = syncContextManager;
        }

        public async Task<T> GetFormAsync<T>()
            where T : Form
        {
            // We are throttling this because there is only one gui thread
            await _semaphore.WaitAsync();

            var form = await _syncContextManager.SynchronizationContext.InvokeAsync(() => _serviceProvider.GetService<T>());

            _semaphore.Release();

            return form;
        }

        public Task<Form> GetMainFormAsync() => Task.FromResult(_applicationContext.MainForm);
    }
}