using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class EventExchange : IDisposable
    {
        readonly IComponent _logger;
        readonly object SyncRoot = new object();
        int _nesting = -1;
        long _nextEventId;
        public int Nesting => _nesting;

        readonly IDictionary<Type, object> _registrations = new Dictionary<Type, object>();
        readonly IDictionary<Type, IList<IComponent>> _subscriptions = new Dictionary<Type, IList<IComponent>>();
        readonly IDictionary<IComponent, IList<Type>> _subscribers = new Dictionary<IComponent, IList<Type>>();
        readonly Stack<HashSet<IComponent>> _dispatchLists = new Stack<HashSet<IComponent>>();
        readonly Queue<(IEvent, object)> _queuedEvents = new Queue<(IEvent, object)>();

#if DEBUG
        // ReSharper disable once CollectionNeverQueried.Local
        readonly IList<IEvent> _frameEvents = new List<IEvent>();
        IList<IComponent> _sortedSubscribersCached;
        public IList<IComponent> SortedSubscribers
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_sortedSubscribersCached?.Count != _subscribers.Count)
                        _sortedSubscribersCached = _subscribers.Keys.OrderBy(x => x.ToString()).ToList();
                    return _sortedSubscribersCached;
                }
            }
        }
#endif

        public void Dispose()
        {
            lock(SyncRoot)
                foreach (var disposableSystem in _registrations.Values.OfType<IDisposable>())
                    disposableSystem.Dispose();
        }

        public EventExchange(IComponent logger)
        {
            _logger = logger;
            Attach(_logger);
        }

        public bool Contains(IComponent component) { lock(SyncRoot) return _subscribers.ContainsKey(component); }

        void Collect(HashSet<IComponent> subscribers, Type type, Type[] interfaces)
        {
            lock (SyncRoot)
            {
                if (_subscriptions.TryGetValue(type, out var tempSubscribers))
                {
                    subscribers.EnsureCapacity(tempSubscribers.Count);
                    foreach (var subscriber in tempSubscribers)
                        subscribers.Add(subscriber);
                }

                foreach(var @interface in interfaces)
                    if (_subscriptions.TryGetValue(@interface, out var interfaceSubscribers))
                        foreach (var subscriber in interfaceSubscribers)
                            subscribers.Add(subscriber);
            }
        }

        public void Raise(IEvent e, object sender, bool includeParent = true)
        {
            bool verbose = e is IVerboseEvent;
            if (!verbose)
            {
                Interlocked.Increment(ref _nesting);
                _logger.Receive(e, sender);
            }

#if DEBUG
            if (e is BeginFrameEvent) _frameEvents.Clear();
            else _frameEvents.Add(e);
#endif

            var type = e.GetType();
            var interfaces = type.GetInterfaces();
            long eventId = Interlocked.Increment(ref _nextEventId);
            string eventText = null;

            if (CoreTrace.Log.IsEnabled())
            {
                eventText = e.ToString();
                if (verbose) CoreTrace.Log.StartRaiseVerbose(eventId, _nesting, e.GetType().Name, eventText);
                else CoreTrace.Log.StartRaise(eventId, _nesting, e.GetType().Name, eventText);
            }

            HashSet<IComponent> subscribers;
            lock (SyncRoot)
            {
                if (!_dispatchLists.TryPop(out subscribers))
                    subscribers = new HashSet<IComponent>();
            }

#if DEBUG
            if (e is BeginFrameEvent) _frameEvents.Clear();
#endif
            Collect(subscribers, type, interfaces);

            foreach (var subscriber in subscribers)
                subscriber.Receive(e, sender);

            if (eventText != null)
            {
                if (verbose) CoreTrace.Log.StopRaiseVerbose(eventId, _nesting, e.GetType().Name, eventText, subscribers.Count);
                else CoreTrace.Log.StopRaise(eventId, _nesting, e.GetType().Name, eventText, subscribers.Count);
            }

            subscribers.Clear();
            _dispatchLists.Push(subscribers);

            if (!verbose)
                Interlocked.Decrement(ref _nesting);
        }

        public void Enqueue(IEvent e, object sender) => _queuedEvents.Enqueue((e, sender));

        public void FlushQueuedEvents()
        {
            while (_queuedEvents.Count > 0)
            {
                var (e, sender) = _queuedEvents.Dequeue();
                Raise(e, sender);
            }
        }

        public EventExchange Attach(IComponent component)
        {
            PerfTracker.StartupEvent($"Attaching {component.GetType().Name}");
            component.Attach(this);
            PerfTracker.StartupEvent($"Attached {component.GetType().Name}");
            return this;
        }

        public void Subscribe<T>(IComponent subscriber) { Subscribe(typeof(T), subscriber); }
        public void Subscribe(Type eventType, IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                {
                    if (subscribedTypes.Contains(eventType))
                        return;
                }
                else
                {
                    _subscribers[subscriber] = new List<Type>();
                }

                if (eventType != null)
                {
                    if (!_subscriptions.ContainsKey(eventType))
                        _subscriptions.Add(eventType, new List<IComponent>());

                    _subscriptions[eventType].Add(subscriber);
                    _subscribers[subscriber].Add(eventType);
                }
            }
        }

        public void Unsubscribe(IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedEventTypes))
                    return;

                foreach (var type in subscribedEventTypes.ToList())
                    _subscriptions[type].Remove(subscriber);

                _subscribers.Remove(subscriber);
            }
        }

        public void Unsubscribe<T>(IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedEventTypes))
                    return;

                if (subscribedEventTypes.Remove(typeof(T)))
                    _subscriptions[typeof(T)].Remove(subscriber);
            }
        }

        public EventExchange Register<T>(T system) => Register(typeof(T), system);
        public EventExchange Register(Type type, object system)
        {
            bool doAttach;
            lock (SyncRoot)
            {
                if (_registrations.ContainsKey(type))
                {
                    if (_registrations[type] != system)
                        throw new InvalidOperationException(
                            "Only one instance can be registered per type / interface in a given exchange.");
                }
                else _registrations.Add(type, system);

                doAttach = system is IComponent component && !_subscribers.ContainsKey(component);
            }

            if (doAttach)
                Attach((IComponent)system);

            return this;
        }

        public void Unregister<T>(T system) => Unregister(typeof(T), system);
        public void Unregister(Type type, object system)
        {
            lock (SyncRoot)
            {
                if (_registrations.TryGetValue(type, out var current) && current == system)
                    _registrations.Remove(type);
            }
        }

        public T Resolve<T>()
        {
            lock (SyncRoot)
                return _registrations.TryGetValue(typeof(T), out var result) ? (T)result : default;
        }

        public IEnumerable EnumerateRecipients(Type eventType)
        {
            lock (SyncRoot)
            {
                var subscribers = new HashSet<IComponent>();
                var interfaces = eventType.GetInterfaces();
                Collect(subscribers, eventType, interfaces);
                return subscribers.ToList();
            }
        }
    }
}
