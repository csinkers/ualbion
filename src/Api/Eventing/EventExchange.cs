using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if DEBUG
#endif

#pragma warning disable CA1030 // Use events where appropriate
namespace UAlbion.Api.Eventing
{
    /// <summary>
    /// An event exchange responsible for efficiently distributing events to all
    /// components that have registered handlers for the event. Also acts as a 
    /// service locator, via the Register / Resolve methods.
    /// </summary>
    public sealed class EventExchange : IDisposable
    {
        readonly object _syncRoot = new();
        readonly ILogExchange _logExchange;
        readonly PooledThreadSafe<List<Handler>> _dispatchLists = new(() => new List<Handler>(), x => x.Clear());
        readonly DoubleBuffered<List<(IEvent, object)>> _queuedEvents = new(() => new List<(IEvent, object)>());
        readonly IDictionary<Type, object> _registrations = new Dictionary<Type, object>();
        readonly IDictionary<Type, List<Handler>> _subscriptions = new Dictionary<Type, List<Handler>>();
        readonly IDictionary<IComponent, List<Handler>> _subscribers = new Dictionary<IComponent, List<Handler>>();

        int _nesting = -1;
        long _nextEventId;
        public int Nesting => _nesting;
        public string Name { get; set; }
        public override string ToString() => Name ?? "EventExchange";

#if DEBUG
        // ReSharper disable once CollectionNeverQueried.Local
        readonly List<IEvent> _frameEvents = new();
        List<IComponent> _sortedSubscribersCached = new();
        public List<IComponent> SortedSubscribers // Just for debugging
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_sortedSubscribersCached?.Count != _subscribers.Count)
                        _sortedSubscribersCached = _subscribers.Keys.OrderBy(x => x.ToString()).ToList();
                    return _sortedSubscribersCached;
                }
            }
        }
#endif

        public EventExchange(ILogExchange logExchange = null)
        {
            _logExchange = logExchange;
            if (_logExchange != null)
                Attach(_logExchange);
        }

        public void Dispose()
        {
            lock (_syncRoot)
                foreach (var disposableSystem in _registrations.Values.OfType<IDisposable>())
                    disposableSystem.Dispose();
        }

        public EventExchange Attach(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            // Stopwatch sw = Stopwatch.StartNew();
            component.Attach(this);
            // PerfTracker.StartupEvent($"Attached {component.GetType().Name} in {sw.ElapsedMilliseconds}ms");
            return this;
        }

        public void Enqueue(IEvent e, object sender) { lock (_syncRoot) _queuedEvents.Front.Add((e, sender)); }

        public void FlushQueuedEvents()
        {
            _queuedEvents.Swap();
            foreach (var (e, sender) in _queuedEvents.Back)
                Raise(e, sender);
            _queuedEvents.Back.Clear();
        }

        void Collect(List<Handler> subscribers, Type eventType) // Must be called from inside lock(_syncRoot)!
        {
            if (!_subscriptions.TryGetValue(eventType, out var tempSubscribers)) 
                return;

            if (subscribers.Capacity < tempSubscribers.Count)
                subscribers.Capacity = tempSubscribers.Count;

            foreach (var subscriber in tempSubscribers)
                subscribers.Add(subscriber);
        }

        public void Raise<T>(T e, object sender) where T : IEvent => RaiseInternal(e, sender, null);
        public int RaiseAsync(IAsyncEvent e, object sender, Action continuation) => RaiseInternal(e, sender, continuation);
        public int RaiseAsync<T>(IAsyncEvent<T> e, object sender, Action<T> continuation) => RaiseInternal(e, sender, continuation);

        int RaiseInternal(IEvent e, object sender, object continuation) // This method is performance critical, memory allocations should be avoided etc.
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            bool verbose = e is IVerboseEvent;
            long eventId = Interlocked.Increment(ref _nextEventId);
            string eventText = LogRaise(e, verbose, eventId, sender);

            List<Handler> handlers = _dispatchLists.Borrow();
            lock (_syncRoot)
            {
#if DEBUG
                if (e is BeginFrameEvent) _frameEvents.Clear();
                else _frameEvents.Add(e);
#endif
                Collect(handlers, e.GetType());
            }

            int inProgressHandlers = 0;
            foreach (var handler in handlers)
                if (sender != handler.Component)
                    if (handler.Invoke(e, continuation))
                        inProgressHandlers++;

            _dispatchLists.Return(handlers);
            LogRaiseEnd(e, verbose, eventId, eventText, handlers.Count);
            return inProgressHandlers;
        }

        string LogRaise(IEvent e, bool verbose, long eventId, object sender)
        {
            if (!verbose)
            { // Nesting level helps identify which events were caused by other events when reading the console window
                Interlocked.Increment(ref _nesting);
                _logExchange?.Receive(e, sender);
            }

            string eventText = null;
            if (CoreTrace.Log.IsEnabled())
            {
                if (verbose)
                {
                    eventText =  e.GetType().Name;
                    CoreTrace.Log.StartRaiseVerbose(eventId, _nesting, e.GetType().Name, eventText);
                }
                else if (e is LogEvent log)
                {
                    // ReSharper disable ExplicitCallerInfoArgument
                    eventText = log.Message;
                    switch (log.Severity)
                    {
                        case LogLevel.Info:     CoreTrace.Log.Info("Log", log.Message, log.File, log.Member, log.Line ?? 0); break;
                        case LogLevel.Warning:  CoreTrace.Log.Warning("Log", log.Message, log.File, log.Member, log.Line ?? 0); break;
                        case LogLevel.Error:    CoreTrace.Log.Error("Log", log.Message, log.File, log.Member, log.Line ?? 0); break;
                        case LogLevel.Critical: CoreTrace.Log.Critical("Log", log.Message, log.File, log.Member, log.Line ?? 0); break;
                    }
                    // ReSharper restore ExplicitCallerInfoArgument
                }
                else
                {
                    eventText = e.ToString();
                    CoreTrace.Log.StartRaise(eventId, _nesting, e.GetType().Name, eventText);
                }
            }

            return eventText;
        }

        void LogRaiseEnd(IEvent e, bool verbose, long eventId, string eventText, int handlerCount)
        {
            if (eventText != null)
            {
                if (verbose) CoreTrace.Log.StopRaiseVerbose(eventId, _nesting, e.GetType().Name, eventText, handlerCount);
                else CoreTrace.Log.StopRaise(eventId, _nesting, e.GetType().Name, eventText, handlerCount);
            }

            if (!verbose)
                Interlocked.Decrement(ref _nesting);
        }

        public void Subscribe<T>(IComponent component) where T : IEvent 
            => Subscribe(new Handler<T>(e => component.Receive(e, null), component));

        bool CheckForDoubleRegistration(Handler handler, List<Handler> subscribedTypes)
        {
            foreach (var x in subscribedTypes)
            {
                if (x.Type != handler.Type)
                    continue;

                Raise(new LogEvent(LogLevel.Error,
                    $"Component of type \"{handler.Component.GetType()}\" tried to register " +
                    $"handler for event {handler.Type}, but it has already registered a handler for that event."), this);
                return true;
            }

            return false;
        }

        public void Subscribe(Handler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_syncRoot)
            {
                if (!handler.ShouldSubscribe)
                    return;

                if (_subscribers.TryGetValue(handler.Component, out var subscribedTypes))
                {
                    if (CheckForDoubleRegistration(handler, subscribedTypes))
                        return;
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
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var handlersForSubscriber))
                    return;

                foreach (var handler in handlersForSubscriber)
                    _subscriptions[handler.Type].Remove(handler);

                _subscribers.Remove(subscriber);
            }
        }

        public void Unsubscribe<T>(IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var handlersForSubscriber))
                    return;

                Handler handler = null;
                foreach (var x in handlersForSubscriber)
                {
                    if (x.Type == typeof(T))
                    {
                        handler = x;
                        break;
                    }
                }

                if (handler == null)
                    return;

                handlersForSubscriber.Remove(handler);
                _subscriptions[typeof(T)].Remove(handler);
            }
        }

        public EventExchange Register<T>(T system) => Register(typeof(T), system);
        public EventExchange Register(Type type, object system, bool attach = true)
        {
            lock (_syncRoot)
            {
                if (_registrations.ContainsKey(type))
                {
                    if (_registrations[type] != system)
                        throw new InvalidOperationException(
                            "Only one instance can be registered per type / interface in a given exchange.");
                    attach = false;
                }
                else
                {
                    _registrations.Add(type, system);
                    attach &= system is IComponent component && !_subscribers.ContainsKey(component);
                }
            }

            if (attach)
                Attach((IComponent)system);

            return this;
        }

        public void Unregister(object system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            Unregister(system.GetType(), system);
            foreach (var i in system.GetType().GetInterfaces())
                Unregister(i, system);
        }

        public void Unregister(Type type, object system)
        {
            lock (_syncRoot)
            {
                if (_registrations.TryGetValue(type, out var current) && current == system)
                    _registrations.Remove(type);
            }
        }

        public T Resolve<T>()
        {
            lock (_syncRoot)
                return _registrations.TryGetValue(typeof(T), out var result) ? (T)result : default;
        }

        public IEnumerable<IComponent> EnumerateRecipients(Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            lock (_syncRoot)
            {
                var subscribers = new List<Handler>();
                Collect(subscribers, eventType);
                return subscribers.Select(x => x.Component).ToList(); // Fine to allocate here - not called every frame.
            }
        }
    }
}
#pragma warning restore CA1030 // Use events where appropriate
