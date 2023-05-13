using System;
using System.Collections.Generic;
using UAlbion.Api.Settings;

namespace UAlbion.Api.Eventing;

#pragma warning disable CA1716 // Identifiers should not match keywords
public interface IAdHocComponentHelper
{
    object Context { get; }
    EventExchange Exchange { get; }
    IReadOnlyList<IComponent> Children { get; }
    T Resolve<T>();
    T TryResolve<T>();

#pragma warning disable CA1030
    void Raise<T>(T @event) where T : IEvent;
    int RaiseAsync(IAsyncEvent @event, Action continuation);
    int RaiseAsync<T>(IAsyncEvent<T> @event, Action<T> continuation);
#pragma warning restore CA1030

    void Enqueue(IEvent @event);
    void Distribute<T>(ICancellableEvent @event, IEnumerable<T> targets, Func<T, IComponent> projection);
    void On<T>(Action<T> callback) where T : IEvent;
    void OnAsync<T>(AsyncMethod<T> callback) where T : IAsyncEvent;
    void OnAsync<TEvent, TReturn>(AsyncMethod<TEvent, TReturn> callback) where TEvent : IAsyncEvent<TReturn>;
    void OnDirectCall<T>(Action<T> callback) where T : IEvent;
    void After<T>(Action<T> callback) where T : IEvent;
    void AfterAsync<T>(AsyncMethod<T> callback) where T : IAsyncEvent;
    void AfterAsync<TEvent, TReturn>(AsyncMethod<TEvent, TReturn> callback) where TEvent : IAsyncEvent<TReturn>;
    void Off<T>();
    void Detach();
    T AttachChild<T>(T child) where T : IComponent;
    void RemoveAllChildren();
    void RemoveChild(IComponent child);
    T Var<T>(IVar<T> varInfo);
    void Verbose(string msg);
    void Info(string msg);
    void Warn(string msg);
    void Error(string msg);
    void Critical(string msg);
}
#pragma warning restore CA1716 // Identifiers should not match keywords
