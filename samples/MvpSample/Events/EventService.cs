using System.Collections.Concurrent;

namespace MvpSample.Events
{
    public interface IEvent { }

    public interface IEventService
    {
        void Publish<T>(T message) where T : IEvent;

        void Subscribe<T>(Action<T> action) where T : IEvent;

        void UnsubscribeAll<T>() where T : IEvent;

        void Unsubscribe<T>(Action<T> action) where T : IEvent;
    }

    internal class EventService : IEventService, IDisposable
    {
        private readonly ConcurrentDictionary<Type, List<object>> _subscriptions = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void Publish<T>(T message)
            where T : IEvent
        {
            if (_subscriptions.TryGetValue(typeof(T), out List<object>? subscribers))
            {
                // We are marsharling these subscribed actions to the thread pool
                Task.Run(() =>
                {
                    foreach (var subscriber in subscribers.ToArray())
                    {
                        ((Action<T>)subscriber)(message);
                    }
                });
            }
        }

        public void Subscribe<T>(Action<T> action)
            where T : IEvent
        {
            var subscribers = _subscriptions.GetOrAdd(typeof(T), t => new List<object>());

            _semaphore.Wait();
            subscribers.Add(action);
            _semaphore.Release();
        }

        public void UnsubscribeAll<T>()
            where T : IEvent
        {
            if (_subscriptions.TryGetValue(typeof(T), out List<object>? _))
            {
                _semaphore.Wait();
                _subscriptions.Remove(typeof(T), out _);
                _semaphore.Release();
            }
        }

        public void Unsubscribe<T>(Action<T> action)
            where T : IEvent
        {
            if (_subscriptions.TryGetValue(typeof(T), out List<object>? subscribers))
            {
                _semaphore.Wait();
                subscribers.Remove(action);
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _subscriptions.Clear();
        }
    }
}