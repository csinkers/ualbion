using System;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core.Tests
{
    public class BasicEvent : Event { }

    public class BasicComponent : Component
    {
        public BasicComponent() => On<BasicEvent>(_ => Handled++);
        public int Handled { get; private set; }
        public T CallResolve<T>() => Resolve<T>();
        public void CallRaise(IEvent e) => Raise(e);
        public void EnableHandler() => On<BasicEvent>(_ => Handled++);
        public void DisableHandler() => Off<BasicEvent>();
        public void Add(IComponent child) => AttachChild(child);
        public void Remove(IComponent child) => RemoveChild(child);
        public void RemoveAll() => RemoveAllChildren();
    }

    public class BasicLogExchange : ILogExchange
    {
        public void Attach(EventExchange exchange) { }
        public void Detach() { }
        public void Receive(IEvent @event, object sender) { }
        public bool IsActive { get; set; }
        public void EnqueueEvent(IEvent e) { }
        public event EventHandler<LogEventArgs> Log;
    }
}
