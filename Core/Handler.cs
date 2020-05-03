using System;
using UAlbion.Api;

namespace UAlbion.Core
{
    abstract class Handler
    {
        public Type Type { get; }
        protected Handler(Type type) { Type = type; }
        public abstract void Invoke(IEvent @event);
    }

    class Handler<TEvent> : Handler
    {
        readonly Action<TEvent> _callback;
        public Handler(Action<TEvent> callback) : base(typeof(TEvent)) => _callback = callback;
        public override void Invoke(IEvent @event) => _callback((TEvent)@event);
    }
}