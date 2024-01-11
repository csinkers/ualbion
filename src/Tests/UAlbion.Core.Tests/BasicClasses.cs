using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Tests;

public class BasicEvent : Event { }
public class BasicAsyncEvent : Event { }
public class BoolAsyncEvent : Event, IQueryEvent<bool> { }

public class BasicComponent : Component
{
    public int Seen { get; private set; }
    public int Handled { get; private set; }
    public T CallResolve<T>() => TryResolve<T>();
    public new void Raise<T>(T e) where T : IEvent => base.Raise(e);
    public new AlbionTask RaiseAsync<T>(T e) where T : IEvent => base.RaiseAsync(e);
    public new AlbionTask<T> RaiseQueryAsync<T>(IQueryEvent<T> e) => base.RaiseQueryAsync(e);
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

    public void AddAsyncHandler<T>(Func<T, AlbionTask> handler) where T : IEvent
    {
        OnAsync<T>(async e =>
        {
            Seen++;
            await handler(e);
            Handled++;
        });
    }

    public void AddAsyncHandler<TEvent, TReturn>(Func<TEvent, AlbionTask<TReturn>> handler) where TEvent : IQueryEvent<TReturn>
    {
        OnQueryAsync<TEvent, TReturn>(async e =>
        {
            Seen++;
            var result = await handler(e);
            Handled++;
            return result;
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

    public void Receive(IEvent e, object sender)
    {
        Log?.Invoke(this, new LogEventArgs
        {
            Time = DateTime.Now,
            Nesting = 0,
            Message = e.ToString(),
            Color = Console.ForegroundColor = ConsoleColor.Gray,
        });
    }
    public bool IsActive { get; set; }
    public int ComponentId => -1;
    public void EnqueueEvent(IEvent e) { }
    public event EventHandler<LogEventArgs> Log;
}