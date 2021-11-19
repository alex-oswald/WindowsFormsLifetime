using System;
using System.Threading;
using System.Threading.Tasks;

namespace OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
{
    internal static class SynchronizationContextExtensions
    {
        public static void Invoke(this SynchronizationContext context, Action action)
        {
            context.Send(delegate {
                action();
            }, null);
        }

        public static TResult Invoke<TResult>(this SynchronizationContext context, Func<TResult> func)
        {
            TResult result = default;
            context.Send(delegate
            {
                result = func();
            }, null);
            return result;
        }

        public static Task<TResult> InvokeAsync<TResult>(this SynchronizationContext context, Func<TResult> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            context.Post(delegate {
                try
                {
                    TResult result = func();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }, null);
            return tcs.Task;
        }

        public static Task<TResult> InvokeAsync<TResult, TInput>(this SynchronizationContext context, Func<TInput, TResult> func, TInput input)
        {
            var tcs = new TaskCompletionSource<TResult>();
            context.Post(delegate {
                try
                {
                    TResult result = func(input);
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }, null);
            return tcs.Task;
        }
    }
}