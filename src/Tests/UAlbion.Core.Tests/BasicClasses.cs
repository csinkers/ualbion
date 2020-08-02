using System;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core.Tests
{
    public class BasicEvent : Event { }
    public class BasicAsyncEvent : Event, IAsyncEvent<bool> { }

    public class BasicComponent : Component
    {
        public int Seen { get; private set; }
        public int Handled { get; private set; }
        public T CallResolve<T>() => Resolve<T>();
        public new void Raise(IEvent e) => base.Raise(e);
        public new int RaiseAsync(IAsyncEvent e, Action continuation) => base.RaiseAsync(e, continuation);
        public new int RaiseAsync<T>(IAsyncEvent<T> e, Action<T> continuation) => base.RaiseAsync(e, continuation);
        public new void Enqueue(IEvent e) => base.Enqueue(e);
        public void AddHandler<T>(Action<T> handler) where T : IEvent
        {
            On<T>(e =>
            {
                Seen++;
                handler(e);
                Handled++;
            });
        }

        public void AddAsyncHandler<T>(Func<T, Action, bool> handler) where T : IAsyncEvent
        {
            OnAsync<T>((e,c) =>
            {
                Seen++;
                return handler(e, () => { Handled++; c(); });
            });
        }

        public void AddAsyncHandler<TEvent, TReturn>(Func<TEvent, Action<TReturn>, bool> handler) where TEvent : IAsyncEvent<TReturn>
        {
            OnAsync<TEvent, TReturn>((e,c) =>
            {
                Seen++;
                return handler(e, x => { Handled++; c(x); });
            });
        }

        public void RemoveHandler<T>() => Off<T>();
        public void AddChild(IComponent child) => AttachChild(child);
        public new void RemoveChild(IComponent child) => base.RemoveChild(child);
        public void RemoveAll() => RemoveAllChildren();
    }

    public class BasicLogExchange : ILogExchange
    {
        public void Attach(EventExchange exchange) { }
        public void Remove() { }
        public void Receive(IEvent @event, object sender) { }
        public bool IsActive { get; set; }
        public void EnqueueEvent(IEvent e) { }
        public event EventHandler<LogEventArgs> Log;
    }
}
