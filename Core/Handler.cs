using System;
using UAlbion.Api;

namespace UAlbion.Core
{
    public abstract class Handler
    {
        public Type Type { get; }
        public IComponent Component { get; }
        protected Handler(Type type, IComponent component)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }
        public abstract void Invoke(IEvent @event);
        public override string ToString() => $"H<{Component.GetType().Name}, {Type.Name}>";
    }

    public class Handler<TEvent> : Handler
    {
        public Action<TEvent> Callback { get; }
        public Handler(Action<TEvent> callback, IComponent component) : base(typeof(TEvent), component) => Callback = callback;
        public override void Invoke(IEvent @event) => Callback((TEvent)@event);
    }
}