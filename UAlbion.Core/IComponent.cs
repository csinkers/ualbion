using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Core
{
    public interface IComponent
    {
        void Attach(EventExchange exchange);
        void Receive(IEvent @event, object sender);
    }

    public abstract class Component : IComponent
    {
        protected abstract class Handler
        {
            public Type Type { get; }

            protected Handler(Type type) { Type = type; }

            public abstract void Invoke(Component instance, IEvent @event);
        }

        protected class Handler<TInstance, TEvent>  : Handler where TInstance : Component
        {
            readonly Action<TInstance, TEvent> _callback;
            public Handler(Action<TInstance, TEvent> callback) : base(typeof(TEvent)) { _callback = callback; }
            public override void Invoke(Component instance, IEvent @event) { _callback((TInstance)instance, (TEvent)@event); }
        }

        readonly IDictionary<Type, Handler> _handlers;
        protected EventExchange Exchange { get; private set; }

        protected Component(IList<Handler> handlers)
        {
            _handlers = handlers.ToDictionary(x => x.Type, x => x);
        }

        public void Attach(EventExchange exchange)
        {
            Exchange = exchange;
            foreach (var kvp in _handlers)
                exchange.Subscribe(kvp.Key, this);
        }

        public void Receive(IEvent @event, object sender)
        {
            if (_handlers.TryGetValue(@event.GetType(), out var handler))
                handler.Invoke(this, @event);
        }
    }
}
