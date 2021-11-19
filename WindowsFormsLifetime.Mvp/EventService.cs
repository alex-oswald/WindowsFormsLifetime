using System.Collections.Concurrent;

namespace WindowsFormsLifetime.Mvp
{
    public interface IEvent { }

    public interface IEventService
    {
        Task PublishAsync<T>(T message) where T : IEvent;

        Task SubscribeAsync<T>(Action<T> action) where T : IEvent;

        Task UnsubscribeAllAsync<T>() where T : IEvent;

        Task UnsubscribeAsync<T>(Action<T> action) where T : IEvent;
    }

    internal class EventService : IEventService, IDisposable
    {
        private readonly ConcurrentDictionary<Type, List<object>> _subscriptions = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task PublishAsync<T>(T message)
            where T : IEvent
        {
            await _semaphore.WaitAsync();

            if (_subscriptions.TryGetValue(typeof(T), out List<object>? subscribers))
            {
                foreach (var subscriber in subscribers.ToArray())
                {
                    ((Action<T>)subscriber)(message);
                }
            }

            _semaphore.Release();
        }

        public async Task SubscribeAsync<T>(Action<T> action)
            where T : IEvent
        {
            await _semaphore.WaitAsync();

            var subscribers = _subscriptions.GetOrAdd(typeof(T), t => new List<object>());
            subscribers.Add(action);

            _semaphore.Release();
        }

        public async Task UnsubscribeAllAsync<T>()
            where T : IEvent
        {

            await _semaphore.WaitAsync();

            if (_subscriptions.TryGetValue(typeof(T), out List<object>? _))
            {
                _subscriptions.Remove(typeof(T), out _);
            }

            _semaphore.Release();
        }

        public async Task UnsubscribeAsync<T>(Action<T> action)
            where T : IEvent
        {
            await _semaphore.WaitAsync();

            if (_subscriptions.TryGetValue(typeof(T), out List<object>? subscribers))
            {
                subscribers.Remove(action);
            }

            _semaphore.Release();
        }

        public void Dispose()
        {
            _subscriptions.Clear();
        }
    }
}