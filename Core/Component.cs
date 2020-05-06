using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    /// <summary>
    /// Component is effectively the base class of all game objects.
    /// Each component belongs to a single EventExchange, and ideally communicates with
    /// other components solely via sending and receiving events through its exchange.
    ///
    /// In general, direct calls should only be used for read-only information retrieval, and
    /// where possible should be done via Resolve to obtain an interface to game sub-systems.
    /// Anything that modifies data, has side effects etc should be performed using an event.
    ///
    /// This has benefits for tracing, reproducibility and modularity, and also provides a
    /// convenient interface for console commands, key-bindings etc.
    /// </summary>
    public abstract class Component : IComponent
    {
        static int _nesting = 0;
        readonly IDictionary<Type, Handler> _handlers = new Dictionary<Type, Handler>();
        bool _isActive = true;
        bool _isSubscribed;

        protected EventExchange Exchange { get; private set; } // N.B. will be null until subscribed.
        protected IList<IComponent> Children { get; } = new List<IComponent>(); // Primary purpose of children is ensuring that the children are also detached when the parent component is.

        protected T Resolve<T>() => Exchange.Resolve<T>(); // Convenience method to save a bit of typing
        protected void Raise(IEvent @event) => Exchange?.Raise(@event, this);
        protected void Enqueue(IEvent @event) => Exchange?.Enqueue(@event, this);
        protected virtual void Subscribed() { }
        protected virtual void Unsubscribed() { }

        protected void On<T>(Action<T> callback)
        {
            if (_handlers.ContainsKey(typeof(T)))
                return;

            var handler = new Handler<T>(callback, this);
            _handlers.Add(typeof(T), handler);
            if (_isSubscribed)
                Exchange.Subscribe(handler);
        }

        protected void Off<T>()
        {
            if (_handlers.Remove(typeof(T)) && _isSubscribed)
                Exchange.Unsubscribe<T>(this);
        }

        protected T AttachChild<T>(T child) where T : IComponent
        {
            if (_isActive)
                Exchange?.Attach(child);
            Children.Add(child);
            return child;
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (value == _isActive)
                    return;

                _isActive = value;

                if (value) Attach(Exchange);
                else Detach();
            }
        }

        public void Attach(EventExchange exchange)
        {
            if (_isSubscribed)
                return;

            Exchange = exchange;

            if (!_isActive)
                return;

            _nesting++;
            Console.WriteLine("+".PadLeft(_nesting) + ToString());
            foreach (var child in Children)
                child.Attach(exchange);
            _nesting--;

            // exchange.Subscribe(null, this); // Ensure we always get added to the subscriber list, even if this component only uses subscription notifications.
            foreach (var kvp in _handlers)
                exchange.Subscribe(kvp.Value);
            _isSubscribed = true;
            Subscribed();
        }

        public void Detach()
        {
            if (Exchange == null)
                return;

            _nesting++;
            Console.WriteLine("-".PadLeft(_nesting) + ToString());
            Unsubscribed();

            foreach (var child in Children)
                child.Detach();

            _nesting--;
            Exchange.Unsubscribe(this);
            _isSubscribed = false;
        }

        public void Receive(IEvent @event, object sender)
        {
            if (sender == this || Exchange == null)
                return;

            if (_handlers.TryGetValue(@event.GetType(), out var handler))
                handler.Invoke(@event);
        }
    }
}
