using Microsoft.Extensions.DependencyInjection;

namespace WindowsFormsLifetime;

public interface IFormProvider
{
    /// <summary>
    /// Gets the requested form type and ensures it is created on the UI thread.
    /// </summary>
    /// <typeparam name="T">The form type to get.</typeparam>
    /// <returns>An instance of the form, asynchronously.</returns>
    Task<T> GetFormAsync<T>() where T : Form;

    /// <summary>
    /// Asynchronously creates a form instance with a single constructor parameter on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameter while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1>(T1 param1) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with two constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2>(T1 param1, T2 param2) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with three constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2, T3>(T1 param1, T2 param2, T3 param3) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with four constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with five constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with six constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <param name="param6">The sixth parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with seven constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth constructor parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <param name="param6">The sixth parameter to pass to the form constructor.</param>
    /// <param name="param7">The seventh parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) where TForm : Form;

    /// <summary>
    /// Asynchronously creates a form instance with eight constructor parameters on the UI thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth constructor parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh constructor parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <param name="param6">The sixth parameter to pass to the form constructor.</param>
    /// <param name="param7">The seventh parameter to pass to the form constructor.</param>
    /// <param name="param8">The eighth parameter to pass to the form constructor.</param>
    /// <returns>A task containing the created form instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the form
    /// with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form type cannot be instantiated or when the UI thread is not available.
    /// </exception>
    Task<TForm> GetFormAsync<TForm, T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) where TForm : Form;

    /// <summary>
    /// Gets the requested form type and ensures it is created on the UI thread. Creates the form in the given scope.
    /// </summary>
    /// <typeparam name="T">The form type to get.</typeparam>
    /// <param name="scope">The scope in which the form should be created.</param>
    /// <returns>An instance of the form, asynchronously.</returns>
    Task<T> GetFormAsync<T>(IServiceScope scope) where T : Form;

    Task<Form> GetMainFormAsync();

    /// <summary>
    /// Gets the requested form type on the current thread. Should only be called on the UI thread. All scoped and transient dependencies will be disposed when the form is disposed.
    /// </summary>
    /// <typeparam name="T">The form type to get.</typeparam>
    /// <returns>An instance of the form.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the UI thread is not available.
    /// </exception>
    T GetForm<T>() where T : Form;

    /// <summary>
    /// Synchronously creates a form instance with a single constructor parameter on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameter while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1>(T1 param1) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with two constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2>(T1 param1, T2 param2) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with three constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2, T3>(T1 param1, T2 param2, T3 param3) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with four constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with five constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with six constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <param name="param6">The sixth parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with seven constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth constructor parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <param name="param6">The sixth parameter to pass to the form constructor.</param>
    /// <param name="param7">The seventh parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) where TForm : Form;

    /// <summary>
    /// Synchronously creates a form instance with eight constructor parameters on the current thread.
    /// </summary>
    /// <typeparam name="TForm">The form type to create.</typeparam>
    /// <typeparam name="T1">The type of the first constructor parameter.</typeparam>
    /// <typeparam name="T2">The type of the second constructor parameter.</typeparam>
    /// <typeparam name="T3">The type of the third constructor parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth constructor parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh constructor parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth constructor parameter.</typeparam>
    /// <param name="param1">The first parameter to pass to the form constructor.</param>
    /// <param name="param2">The second parameter to pass to the form constructor.</param>
    /// <param name="param3">The third parameter to pass to the form constructor.</param>
    /// <param name="param4">The fourth parameter to pass to the form constructor.</param>
    /// <param name="param5">The fifth parameter to pass to the form constructor.</param>
    /// <param name="param6">The sixth parameter to pass to the form constructor.</param>
    /// <param name="param7">The seventh parameter to pass to the form constructor.</param>
    /// <param name="param8">The eighth parameter to pass to the form constructor.</param>
    /// <returns>The created form instance.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called from the UI thread. It creates a new service scope for the form
    /// and automatically disposes it when the form is disposed. The method uses <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
    /// to create the form with the provided parameters while resolving other dependencies from the dependency injection container.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the form type cannot be instantiated.
    /// </exception>
    TForm GetForm<TForm, T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) where TForm : Form;

    /// <summary>
    /// Gets the requested form type on the current thread. Should only be called on the UI thread.  Creates the form in the given scope.
    /// </summary>
    /// <typeparam name="T">The form type to get.</typeparam>
    /// <param name="scope">The scope in which the form should be created.</param>
    /// <returns>An instance of the form.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called from a non-UI thread or when the UI thread is not available.
    /// </exception>
    T GetForm<T>(IServiceScope scope) where T : Form;
}
