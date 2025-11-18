using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace UAlbion.Api.Eventing
{
    /// <summary>
    /// An event exchange responsible for efficiently distributing events to all
    /// components that have registered handlers for the event. Also acts as a 
    /// service locator, via the Register / Resolve methods.
    /// </summary>
    public sealed class EventExchange : IDisposable
    {
        readonly Lock _syncRoot = new();
        [DiagIgnore] readonly ILogExchange _logExchange;
        [DiagIgnore] readonly PooledThreadSafe<List<Handler>> _dispatchLists = new(() => [], x => x.Clear());
        [DiagIgnore] readonly DoubleBuffered<List<(IEvent Event, object Sender)>> _queuedEvents = new(() => []);
        readonly Dictionary<Type, object> _registrations = [];
        readonly Dictionary<Type, List<Handler>> _subscriptions = [];
        readonly Dictionary<IComponent, List<Handler>> _subscribers = [];

        [DiagIgnore] int _nesting = -1;
        [DiagIgnore] long _nextEventId;
        [DiagIgnore] public int Nesting => _nesting;
        public string Name { get; set; }
        public override string ToString() => Name ?? "EventExchange";

#if DEBUG
        // ReSharper disable once CollectionNeverQueried.Local
        [DiagIgnore] readonly List<(int Depth, object Event, long TimestampTicks)> _frameEvents = [];
        [DiagIgnore] List<IComponent> _sortedSubscribersCached = [];
        public IReadOnlyList<IComponent> SortedSubscribers // Just for debugging
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_sortedSubscribersCached?.Count != _subscribers.Count)
                        _sortedSubscribersCached = [.._subscribers.Keys.OrderBy(x => x.ToString())];
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
            ArgumentNullException.ThrowIfNull(component);
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

        // [DebuggerHidden, StackTraceHidden]
        public void Raise(IEvent e, object sender) => _ = RaiseInner(e, sender, 0, RaiseInvoker);
        static AlbionTask<int> RaiseInvoker(List<Handler> handlers, IEvent e, object sender, int _)
        {
            foreach (var handler in handlers)
            {
                if (sender == handler.Component) continue;

                switch (handler)
                {
                    case ISyncHandler syncHandler:
                        syncHandler.Invoke(e);
                        break;
                    case IAsyncHandler asyncHandler:
                        asyncHandler.InvokeAsAsync(e); // Any async handlers are started but never awaited
                        break;
                    default:
                        throw new InvalidOperationException($"Could not invoke handler {handler}");
                }
            }

            return AlbionTask.FromResult(0);
        }

        // [DebuggerHidden, StackTraceHidden]
        public async AlbionTask RaiseA(IEvent e, object sender) => _ = await RaiseInner(e, sender, 0, RaiseAInvoker); // TODO: Avoid delegate allocations
        static AlbionTask<Unit> RaiseAInvoker(List<Handler> handlers, IEvent e, object sender, int _) // Waits for all handlers to complete
        {
            AlbionTaskCore<Unit> core = null;

            foreach (var handler in handlers)
            {
                if (sender == handler.Component) continue;

                switch (handler)
                {
                    case ISyncHandler syncHandler:
                        syncHandler.Invoke(e);
                        break;

                    case IAsyncHandler asyncHandler:
                        {
                            var innerTask = asyncHandler.InvokeAsAsync(e);
                            if (innerTask.IsCompleted)
                                break;

                            core ??= new($"RaiseAInvoker helper for {e.GetType().Name} from {sender}");
                            core.OutstandingCompletions++;
                            var core1 = core;
                            innerTask.OnCompleted(() =>
                            {
                                core1.OutstandingCompletions--;
                                if (core1.OutstandingCompletions == 0)
                                    core1.SetResult(Unit.V);
                            });

                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Could not invoke handler {handler}");
                }
            }

            return core?.Task ?? AlbionTask.Unit;
        }

        // [DebuggerHidden, StackTraceHidden]
        public T RaiseQuery<T>(IQueryEvent<T> e, object sender) => RaiseInner(e, sender, 0, RaiseQueryInvoker).GetResult();
        static AlbionTask<T> RaiseQueryInvoker<T>(List<Handler> handlers, IQueryEvent<T> e, object sender, int _)
        {
            bool hasResult = false;
            T result = default;

            foreach (var handler in handlers)
            {
                if (sender == handler.Component) continue;

                switch (handler)
                {
                    case ISyncQueryHandler<T> queryHandler:
                        if (hasResult) throw new InvalidOperationException("Multiple results found in RaiseQuery call");

                        result = queryHandler.Invoke(e);
                        hasResult = true;
                        break;

                    case ISyncHandler syncHandler:
                        syncHandler.Invoke(e);
                        break;
                    case IAsyncHandler asyncHandler:
                        asyncHandler.InvokeAsAsync(e);
                        break; // Any async handlers are started but never awaited
                    default:
                        throw new InvalidOperationException($"Could not invoke handler {handler}");
                }
            }

            if (!hasResult)
                throw new InvalidOperationException("No result found for RaiseQuery call");

            return AlbionTask.FromResult(result);
        }

        // [DebuggerHidden, StackTraceHidden]
        public AlbionTask<T> RaiseQueryA<T>(IQueryEvent<T> e, object sender) => RaiseInner(e, sender, 0, RaiseQueryAInvoker);
        static async AlbionTask<T> RaiseQueryAInvoker<T>(List<Handler> handlers, IQueryEvent<T> e, object sender, int _)
        {
            bool hasResult = false;
            T result = default;

            foreach (var handler in handlers)
            {
                if (sender == handler.Component) continue;

                switch (handler)
                {
                    case ISyncQueryHandler<T> queryHandler:
                        if (hasResult) throw new InvalidOperationException("Multiple results found in RaiseQuery call");

                        result = queryHandler.Invoke(e);
                        hasResult = true;
                        break;

                    case IAsyncQueryHandler<T> queryHandler:
                        if (hasResult) throw new InvalidOperationException("Multiple results found in RaiseQuery call");

                        result = await queryHandler.InvokeAsAsync(e);
                        hasResult = true;
                        break;

                    case ISyncHandler syncHandler:
                        syncHandler.Invoke(e);
                        break;
                    case IAsyncHandler asyncHandler:
                        await asyncHandler.InvokeAsAsync(e);
                        break;
                    default:
                        throw new InvalidOperationException($"Could not invoke handler {handler}");
                }
            }

            if (!hasResult)
                throw new InvalidOperationException("No result found for RaiseQuery call");

            return result;
        }


        delegate AlbionTask<TResult> InvokerFunc<in TEvent, TResult, in TContext>(List<Handler> handlers, TEvent e, object sender, TContext context);

        // [DebuggerHidden, StackTraceHidden]
        AlbionTask<TResult> RaiseInner<TEvent, TResult, TContext>( // This method is performance critical, memory allocations should be avoided etc.
            TEvent e,
            object sender,
            TContext context,
            InvokerFunc<TEvent, TResult, TContext> invoker) where TEvent : IEvent
        {
            ArgumentNullException.ThrowIfNull(e);
            bool verbose = e is IVerboseEvent;
            long eventId = Interlocked.Increment(ref _nextEventId);
            string eventText = LogRaise(e, verbose, eventId, sender);

            List<Handler> handlers = _dispatchLists.Borrow();
            lock (_syncRoot)
            {
#if DEBUG
                if (e is BeginFrameEvent) _frameEvents.Clear();
                else
                {
                    long time = Stopwatch.GetTimestamp();
                    _frameEvents.Add((_nesting, e, time));
                }
#endif
                Collect(handlers, e.GetType());
            }

            var result = invoker(handlers, e, sender, context);

            LogRaiseEnd(e, verbose, eventId, eventText, handlers.Count);

            if (result.IsCompleted)
                _dispatchLists.Return(handlers);
            else
                result.OnCompleted(() => _dispatchLists.Return(handlers));

            return result;
        }

        void Collect(List<Handler> subscribers, Type eventType) // Must be called from inside lock(_syncRoot)!
        {
            if (!_subscriptions.TryGetValue(eventType, out var tempSubscribers)) 
                return;

            if (subscribers.Capacity < tempSubscribers.Count)
                subscribers.Capacity = tempSubscribers.Count;

            List<Handler> postHandlers = null;
            foreach (var subscriber in tempSubscribers)
            {
                if (subscriber.IsPostHandler)
                {
                    postHandlers ??= _dispatchLists.Borrow();
                    postHandlers.Add(subscriber);
                }
                else subscribers.Add(subscriber);
            }

            if (postHandlers != null)
            {
                foreach (var subscriber in postHandlers)
                    subscribers.Add(subscriber);
                _dispatchLists.Return(postHandlers);
            }
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
                    eventText = e.GetType().Name;
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

        public void Subscribe<T>(IComponent component, bool isPostHandler = false) where T : IEvent 
            => Subscribe(new SyncHandler<T>(e => component.Receive(e, null), component, isPostHandler));

        bool CheckForDoubleRegistration(Handler handler, List<Handler> subscribedTypes)
        {
            foreach (var x in subscribedTypes)
            {
                if (x.Type != handler.Type)
                    continue;

                Raise(new LogEvent(LogLevel.Error,
                    $"Component {handler.Component.ComponentId} of type \"{handler.Component.GetType()}\" tried to register " +
                    $"handler for event {handler.Type}, but it has already registered a handler for that event."), this);
                return true;
            }

            return false;
        }

        public void Subscribe(Handler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
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
                    _subscribers[handler.Component] = [];
                }

                if (handler.Type != null)
                {
                    if (!_subscriptions.ContainsKey(handler.Type))
                        _subscriptions.Add(handler.Type, []);

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
                if (!_registrations.TryAdd(type, system))
                {
                    if (_registrations[type] != system)
                        throw new InvalidOperationException("Only one instance can be registered per type / interface in a given exchange.");

                    attach = false;
                }
                else
                {
                    attach &= system is IComponent component && !_subscribers.ContainsKey(component);
                }
            }

            if (attach)
                Attach((IComponent)system);

            return this;
        }

        public void Unregister(object system)
        {
            ArgumentNullException.ThrowIfNull(system);
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
            ArgumentNullException.ThrowIfNull(eventType);
            lock (_syncRoot)
            {
                var subscribers = new List<Handler>();
                Collect(subscribers, eventType);
                return [..subscribers.Select(x => x.Component)]; // Fine to allocate here - not called every frame.
            }
        }
    }
}
