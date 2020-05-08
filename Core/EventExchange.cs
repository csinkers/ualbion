using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    /// <summary>
    /// An event exchange responsible for efficiently distributing events to all
    /// components that have registered handlers for the event. Also acts as a 
    /// service locator, via the Register / Resolve methods.
    /// </summary>
    public class EventExchange : IDisposable
    {
        readonly object SyncRoot = new object();
        readonly ILogExchange _logExchange;
        int _nesting = -1;
        long _nextEventId;
        public int Nesting => _nesting;

        readonly Stack<List<Handler>> _dispatchLists = new Stack<List<Handler>>();
        readonly Queue<(IEvent, object)> _queuedEvents = new Queue<(IEvent, object)>();
        readonly IDictionary<Type, object> _registrations = new Dictionary<Type, object>();
        readonly IDictionary<Type, IList<Handler>> _subscriptions = new Dictionary<Type, IList<Handler>>();
        readonly IDictionary<IComponent, IList<Handler>> _subscribers = new Dictionary<IComponent, IList<Handler>>();

#if DEBUG
        // ReSharper disable once CollectionNeverQueried.Local
        readonly IList<IEvent> _frameEvents = new List<IEvent>();
        IList<IComponent> _sortedSubscribersCached;
        public IList<IComponent> SortedSubscribers // Just for debugging
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

        public EventExchange(ILogExchange logExchange)
        {
            _logExchange = logExchange;
            Attach(_logExchange);
        }

        public void Dispose()
        {
            lock(SyncRoot)
                foreach (var disposableSystem in _registrations.Values.OfType<IDisposable>())
                    disposableSystem.Dispose();
        }

        public EventExchange Attach(IComponent component)
        {
            PerfTracker.StartupEvent($"Attaching {component.GetType().Name}");
            component.Attach(this);
            PerfTracker.StartupEvent($"Attached {component.GetType().Name}");
            return this;
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

        void Collect(List<Handler> subscribers, Type type, Type[] interfaces) // Must be called from inside lock(_syncRoot)!
        {
            if (_subscriptions.TryGetValue(type, out var tempSubscribers))
            {
                if (subscribers.Capacity < tempSubscribers.Count)
                    subscribers.Capacity = tempSubscribers.Count;

                foreach (var subscriber in tempSubscribers)
                    subscribers.Add(subscriber);
            }

            foreach (var @interface in interfaces)
                if (_subscriptions.TryGetValue(@interface, out var interfaceSubscribers))
                    foreach (var subscriber in interfaceSubscribers)
                        subscribers.Add(subscriber);
        }

        public void Raise(IEvent e, object sender)
        {
            bool verbose = e is IVerboseEvent;
            if (!verbose)
            {
                Interlocked.Increment(ref _nesting);
                _logExchange.Receive(e, sender);
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

            List<Handler> handlers;
            lock (SyncRoot)
            {
                if (!_dispatchLists.TryPop(out handlers))
                    handlers = new List<Handler>();
#if DEBUG
                if (e is BeginFrameEvent) _frameEvents.Clear();
#endif
                Collect(handlers, type, interfaces);
            }

            foreach (var handler in handlers)
                if (sender != handler.Component)
                    handler.Invoke(e);

            if (eventText != null)
            {
                if (verbose) CoreTrace.Log.StopRaiseVerbose(eventId, _nesting, e.GetType().Name, eventText, handlers.Count);
                else CoreTrace.Log.StopRaise(eventId, _nesting, e.GetType().Name, eventText, handlers.Count);
            }

            handlers.Clear();
            lock (SyncRoot)
                _dispatchLists.Push(handlers);

            if (!verbose)
                Interlocked.Decrement(ref _nesting);
        }

        public void Subscribe<T>(IComponent component) where T : IEvent 
            => Subscribe(new Handler<T>(e => component.Receive(e, null), component));

        public void Subscribe(Handler handler)
        {
            lock (SyncRoot)
            {
                if (_subscribers.TryGetValue(handler.Component, out var subscribedTypes))
                {
                    if (subscribedTypes.Any(x => x.Type == handler.Type))
                    {
                        Raise(new LogEvent(
                            LogEvent.Level.Error,
                            $"Component of type \"{handler.Component.GetType()}\" tried to register " +
                            $"handler for event {handler.Type}, but it has already registered a handler for that event."),
                            this);
                        return;
                    }
                }
                else
                {
                    _subscribers[handler.Component] = new List<Handler>();
                }

                if (handler.Type != null)
                {
                    if (!_subscriptions.ContainsKey(handler.Type))
                        _subscriptions.Add(handler.Type, new List<Handler>());

                    _subscriptions[handler.Type].Add(handler);
                    _subscribers[handler.Component].Add(handler);
                }
            }
        }

        public void Unsubscribe(IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var handlersForSubscriber))
                    return;

                foreach (var handler in handlersForSubscriber.ToList())
                    _subscriptions[handler.Type].Remove(handler);

                _subscribers.Remove(subscriber);
            }
        }

        public void Unsubscribe<T>(IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var handlersForSubscriber))
                    return;

                var handler = handlersForSubscriber.FirstOrDefault(x => x.Type == typeof(T));
                if (handler == null)
                    return;

                handlersForSubscriber.Remove(handler);
                _subscriptions[typeof(T)].Remove(handler);
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
                var subscribers = new List<Handler>();
                var interfaces = eventType.GetInterfaces();
                Collect(subscribers, eventType, interfaces);
                return subscribers.ToList();
            }
        }
    }
}
