using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public abstract class Component : IComponent
    {
        // Used when null is passed to the constructor so we
        // won't need null checks when accessing _handlers.
        static readonly HandlerSet EmptySet = new HandlerSet();
        protected abstract class Handler
        {
            public Type Type { get; }

            protected Handler(Type type) { Type = type; }

            public abstract void Invoke(Component instance, IEvent @event);
        }

        protected class Handler<TInstance, TEvent> : Handler where TInstance : Component
        {
            readonly Action<TInstance, TEvent> _callback;

            public Handler(Action<TInstance, TEvent> callback)
                : base(typeof(TEvent))
            {
                _callback = callback;
            }

            public override void Invoke(Component instance, IEvent @event)
            {
                _callback((TInstance)instance, (TEvent)@event);
            }
        }

        protected class HandlerSet : Dictionary<Type, Handler>
        {
            public HandlerSet(params Handler[] handlers)
            {
                if (handlers == null)
                    return;
                foreach (var handler in handlers)
                    Add(handler.Type, handler);
            }
        }

        // Helper to reduce verbosity
        protected static Handler H<TComponent, TEvent>(Action<TComponent, TEvent> callback) where TComponent : Component
            => new Handler<TComponent, TEvent>(callback);

        // Usually set to a static HandlerSet, but can also be per-instance is so desired.
        readonly IDictionary<Type, Handler> _handlers; 
        protected EventExchange Exchange { get; private set; } // N.B. will be null until subscribed.
        protected IList<IComponent> Children { get; } = new List<IComponent>();

        protected Component(IDictionary<Type, Handler> handlers)
        {
            _handlers = handlers ?? EmptySet;
            if (!_handlers.ContainsKey(typeof(SubscribedEvent)))
                _handlers.Add(typeof(SubscribedEvent), new Handler<Component, SubscribedEvent>((x, e) => x.Subscribed()));
        }

        public void Attach(EventExchange exchange)
        {
            if (Exchange == exchange)
                return;

            if (Exchange != null)
                throw new InvalidOperationException("A component can only be registered in one exchange at a time.");

            Exchange = exchange;

            foreach (var child in Children)
                child.Attach(exchange);

            foreach (var kvp in _handlers)
                exchange.Subscribe(kvp.Key, this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Resolve<T>() => Exchange.Resolve<T>(); // Convenience method to save a bit of typing

        public void Receive(IEvent @event, object sender)
        {
            if (sender == this)
                return;
            if (_handlers.TryGetValue(@event.GetType(), out var handler))
                handler.Invoke(this, @event);
            else
            {
                var interfaces = @event.GetType().GetInterfaces();
                foreach (var i in interfaces)
                {
                    if (_handlers.TryGetValue(i, out var interfaceHandler))
                    {
                        interfaceHandler.Invoke(this, @event);
                        break;
                    }
                }
            }
        }

        public virtual void Detach()
        {
            if (Exchange == null)
                return;

            foreach (var child in Children)
                child.Detach();

            Exchange.Unsubscribe(this);
            Exchange = null;
        }

        protected virtual void Subscribed() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Raise(IEvent @event) { Exchange?.Raise(@event, this); }
    }
}