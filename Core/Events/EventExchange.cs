using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class EventExchange
    {
        static readonly object _syncRoot = new object();
        readonly IDictionary<Type, IList<IComponent>> _subscriptions = new Dictionary<Type, IList<IComponent>>();
        readonly IDictionary<IComponent, IList<Type>> _subscribers = new Dictionary<IComponent, IList<Type>>();
        readonly IDictionary<Type, object> _registrations = new Dictionary<Type, object>();
        readonly EventExchange _parent;
        readonly IList<EventExchange> _children = new List<EventExchange>();
        readonly SubscribedEvent _subscribedEvent = new SubscribedEvent();
#if DEBUG
        readonly IList<IEvent> _frameEvents = new List<IEvent>();
#endif

        public string Name { get; }
        public bool IsActive { get; set; } = true;

        public override string ToString() => $"EventExchange \"{Name}\" (IsActive={IsActive})";

        public IReadOnlyList<EventExchange> Children { get { lock (_syncRoot) return _children.ToList(); } }

        public EventExchange(string name, EventExchange parent = null)
        {
            Name = name;
            _parent = parent;
            _parent?.AddChild(this);
        }

        void AddChild(EventExchange eventExchange)
        {
            lock(_syncRoot)
                _children.Add(eventExchange);
        }

        public bool Contains(IComponent component) { lock(_syncRoot) return _subscribers.ContainsKey(component); }

        void Collect(HashSet<IComponent> subscribers, Type type, Type[] interfaces)
        {
            lock (_syncRoot)
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

        void CollectExchanges(ISet<EventExchange> exchanges)
        {
            if (!IsActive)
                return;

            if (!exchanges.Add(this))
                return;

            _parent?.CollectExchanges(exchanges);
            foreach (var childExchange in _children)
                childExchange.CollectExchanges(exchanges);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Raise(IEvent e, object sender)
        {
            // Event raising goes both up and down the hierarchy (i.e. all events will be delivered to all interested subscribers on all active exchanges)
            // As such, the number of exchanges should be kept to a minimum. e.g. one global, one for the active scene etc
            if (!IsActive)
                return;

#if DEBUG
            if(e is BeginFrameEvent) _frameEvents.Clear();
            else _frameEvents.Add(e);
#endif

            HashSet<IComponent> subscribers = new HashSet<IComponent>();
            var type = e.GetType();
            var interfaces = type.GetInterfaces();
            string eventText = null;

            if (CoreTrace.Log.IsEnabled())
            {
                eventText = e.ToString();
                if(e is IVerboseEvent) CoreTrace.Log.StartRaiseVerbose(e.GetType().Name, eventText);
                else CoreTrace.Log.StartRaise(e.GetType().Name, eventText);
            }

            var exchanges = new HashSet<EventExchange>();
            lock(_syncRoot)
                CollectExchanges(exchanges);
            foreach(var exchange in exchanges)
                exchange.Collect(subscribers, type, interfaces);

            foreach(var subscriber in subscribers)
                subscriber.Receive(e, sender);

            if (eventText != null)
            {
                if (e is IVerboseEvent) CoreTrace.Log.StopRaiseVerbose(e.GetType().Name, eventText, subscribers.Count);
                else CoreTrace.Log.StopRaise(e.GetType().Name, eventText, subscribers.Count);
            }
        }

        public EventExchange Attach(IComponent component) { component.Attach(this); return this; }
        public void Subscribe<T>(IComponent subscriber) { Subscribe(typeof(T), subscriber); }
        public void Subscribe(Type eventType, IComponent subscriber)
        {
            bool newSubscriber = false;
            lock (_syncRoot)
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

                if (!_subscriptions.ContainsKey(eventType))
                    _subscriptions.Add(eventType, new List<IComponent>());

                _subscriptions[eventType].Add(subscriber);
                _subscribers[subscriber].Add(eventType);
            }
            if (newSubscriber)
                    subscriber.Receive(_subscribedEvent, this);
        }

        public void Unsubscribe(IComponent subscriber)
        {
            lock (_syncRoot)
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
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                subscribedTypes.Remove(typeof(T));
                _subscriptions[typeof(T)].Remove(subscriber);
            }
        }

        public EventExchange Register<T>(T system)
        {
            if(_registrations.ContainsKey(typeof(T)))
                throw new InvalidOperationException("Only one instance can be registered per type / interface in a given exchange.");
            _registrations.Add(typeof(T), system);

            if(system is IComponent component && !_subscribers.ContainsKey(component))
                Attach(component);

            return this;
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
            lock (_syncRoot)
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
