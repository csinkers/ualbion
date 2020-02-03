using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class EventExchange : IDisposable
    {
        static IComponent _logger;
        static int _nesting = -1;
        static readonly object SyncRoot = new object();
        public static int Nesting => _nesting;

        readonly IDictionary<Type, IList<IComponent>> _subscriptions = new Dictionary<Type, IList<IComponent>>();
        readonly IDictionary<IComponent, IList<Type>> _subscribers = new Dictionary<IComponent, IList<Type>>();
        readonly ISet<IComponent> _topLevelSubscribers = new HashSet<IComponent>();
        readonly IDictionary<Type, object> _registrations = new Dictionary<Type, object>();
        readonly ThreadLocal<bool> _isTopLevel = new ThreadLocal<bool>(() => true);
        readonly EventExchange _parent;
        readonly IList<EventExchange> _children = new List<EventExchange>();
#if DEBUG
        // ReSharper disable once CollectionNeverQueried.Local
        readonly IList<IEvent> _frameEvents = new List<IEvent>();
#endif

        public string Name { get; }

        bool _isActive = true;

        public IEnumerable<object> SortedSubscribers { get { lock(SyncRoot) { return _subscribers.Keys.OrderBy(x => x.ToString()).ToList(); } } } 
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                    return;

                _isActive = value;
                if (value)
                {
                    IList<IComponent> subscribers;
                    lock (SyncRoot)
                    {
                        var exchanges = new HashSet<EventExchange>();
                        CollectExchanges(exchanges, false);
                        subscribers = exchanges.SelectMany(x => x._subscribers.Keys).ToList();
                    }

                    foreach (var subscriber in subscribers)
                        if (subscriber.IsSubscribed) // Another components subscribe call may have detached some of the subscribers
                            subscriber.Subscribed();
                }
            }
        }

        public override string ToString() => $"EventExchange \"{Name}\" (IsActive={IsActive})";
        public void Dispose()
        {
            lock(SyncRoot)
                foreach (var disposableSystem in _registrations.Values.OfType<IDisposable>())
                    disposableSystem.Dispose();

            foreach(var child in _children)
                child.Dispose();
            _children.Clear();
        }

        public IReadOnlyList<EventExchange> Children { get { lock (SyncRoot) return _children.ToList(); } }

        public EventExchange(string name, EventExchange parent)
        {
            Name = name;
            _parent = parent;
            _parent?.AddChild(this);
        }

        public EventExchange(string name, IComponent logger)
        {
            Name = name;
            _logger = logger;
            Attach(_logger);
        }

        void AddChild(EventExchange eventExchange)
        {
            lock(SyncRoot)
                _children.Add(eventExchange);
        }

        public bool Contains(IComponent component) { lock(SyncRoot) return _subscribers.ContainsKey(component); }

        void Collect(HashSet<IComponent> subscribers, Type type, Type[] interfaces)
        {
            lock (SyncRoot)
            {
                if (_subscriptions.TryGetValue(type, out var tempSubscribers))
                    foreach (var subscriber in tempSubscribers)
                        subscribers.Add(subscriber);

                foreach(var @interface in interfaces)
                    if (_subscriptions.TryGetValue(@interface, out var interfaceSubscribers))
                        foreach (var subscriber in interfaceSubscribers)
                            subscribers.Add(subscriber);
            }
        }

        void CollectExchanges(ISet<EventExchange> exchanges, bool includeParent = true)
        {
            if (!IsActive)
                return;

            if (!exchanges.Add(this))
                return;

            if(includeParent)
                _parent?.CollectExchanges(exchanges);

            foreach (var childExchange in _children)
                childExchange.CollectExchanges(exchanges);
        }

        public void Raise(IEvent e, object sender)
        {
            // Event raising goes both up and down the hierarchy (i.e. all events will be delivered to all interested subscribers on all active exchanges)
            // As such, the number of exchanges should be kept to a minimum. e.g. one global, one for the active scene etc
            if (!IsActive)
                return;

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

            HashSet<IComponent> subscribers = new HashSet<IComponent>();
            var type = e.GetType();
            var interfaces = type.GetInterfaces();
            string eventText = null;

            if (CoreTrace.Log.IsEnabled())
            {
                eventText = e.ToString();
                if (verbose) CoreTrace.Log.StartRaiseVerbose(_nesting, e.GetType().Name, eventText);
                else CoreTrace.Log.StartRaise(_nesting, e.GetType().Name, eventText);
            }

            var exchanges = new HashSet<EventExchange>();
            lock (SyncRoot)
                CollectExchanges(exchanges);
            foreach (var exchange in exchanges)
            {
#if DEBUG
                if (e is BeginFrameEvent) exchange._frameEvents.Clear();
#endif
                exchange.Collect(subscribers, type, interfaces);
            }

            foreach (var subscriber in subscribers)
                subscriber.Receive(e, sender);

            if (eventText != null)
            {
                if (verbose) CoreTrace.Log.StopRaiseVerbose(_nesting, e.GetType().Name, eventText, subscribers.Count);
                else CoreTrace.Log.StopRaise(_nesting, e.GetType().Name, eventText, subscribers.Count);
            }

            if (!verbose)
                Interlocked.Decrement(ref _nesting);
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
            bool newSubscriber = false;
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
                    newSubscriber = true;
                }

                if (eventType != null)
                {
                    if (!_subscriptions.ContainsKey(eventType))
                        _subscriptions.Add(eventType, new List<IComponent>());

                    _subscriptions[eventType].Add(subscriber);
                    _subscribers[subscriber].Add(eventType);
                }
            }

            if (newSubscriber)
            {
                var wasTopLevel = _isTopLevel.Value;
                if (wasTopLevel)
                    lock(SyncRoot)
                        _topLevelSubscribers.Add(subscriber);

                _isTopLevel.Value = false;
                subscriber.Subscribed();
                _isTopLevel.Value = wasTopLevel;
            }
        }

        public void Unsubscribe(IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                foreach (var type in subscribedTypes.ToList())
                    _subscriptions[type].Remove(subscriber);

                _subscribers.Remove(subscriber);
            }
        }

        public void Unsubscribe<T>(IComponent subscriber)
        {
            lock (SyncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                subscribedTypes.Remove(typeof(T));
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
            // System resolution only goes up
            var exchange = this;
            while (exchange != null)
            {
                if (exchange._registrations.TryGetValue(typeof(T), out var result))
                    return (T)result;

                exchange = exchange._parent;
            }

            return default;
        }

        public void PruneInactiveChildren()
        {
            lock (SyncRoot)
            {
                for (int i = 0; i < _children.Count;)
                {
                    if (!_children[i].IsActive)
                        _children.RemoveAt(i);
                    else i++;
                }
            }
        }
    }
}
