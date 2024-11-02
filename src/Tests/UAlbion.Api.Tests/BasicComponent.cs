using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Api.Tests;

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