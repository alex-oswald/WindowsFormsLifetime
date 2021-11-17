using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
{
    public interface IFormProvider : IWindowsFormsSynchronizationContext
    {
        /// <summary>
        /// Gets the requested form type and ensures it is created on the UI thread.
        /// </summary>
        /// <typeparam name="T">The form type to get.</typeparam>
        /// <returns>An instance of the form, asynchronously.</returns>
        Task<T> GetFormAsync<T>() where T : Form;
    }

    public interface IWindowsFormsSynchronizationContext
    {
        /// <summary>
        /// Gets the <see cref="WindowsFormsSynchronizationContext"/> for the UI thread.
        /// </summary>
        WindowsFormsSynchronizationContext SynchronizationContext { get; }
    }

    public interface IGuiContext
    {
        void Invoke(Action action);

        TResult Invoke<TResult>(Func<TResult> func);

        Task<TResult> InvokeAsync<TResult>(Func<TResult> func);

        Task<TResult> InvokeAsync<TResult, TInput>(Func<TInput, TResult> func, TInput input);
    }

    public class FormProvider : IFormProvider, IGuiContext
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IServiceProvider _serviceProvider;

        public FormProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public WindowsFormsSynchronizationContext SynchronizationContext { get; internal set; }

        public async Task<T> GetFormAsync<T>()
            where T : Form
        {
            // We are throttling this because there is only one gui thread
            await _semaphore.WaitAsync();

            var form = await SynchronizationContext.InvokeAsync(() => _serviceProvider.GetService<T>());

            _semaphore.Release();

            return form;
        }

        public void Invoke(Action action) => SynchronizationContext.Invoke(action);

        public TResult Invoke<TResult>(Func<TResult> func) => SynchronizationContext.Invoke(func);

        public async Task<TResult> InvokeAsync<TResult>(Func<TResult> func) => await SynchronizationContext.InvokeAsync(func);

        public async Task<TResult> InvokeAsync<TResult, TInput>(Func<TInput, TResult> func, TInput input) => await SynchronizationContext.InvokeAsync(func, input);
    }
}
