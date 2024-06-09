using System;
using System.Collections.Generic;
using UAlbion.Api.Settings;

namespace UAlbion.Api.Eventing;

#pragma warning disable CA1716 // Identifiers should not match keywords
public interface IAdHocComponentHelper
{
    object ThreadContext { get; }
    EventExchange Exchange { get; }
    IReadOnlyList<IComponent> Children { get; }
    T Resolve<T>();
    T TryResolve<T>();

#pragma warning disable CA1030
    void Raise<T>(T e) where T : IEvent;
    AlbionTask RaiseAsync<T>(T e) where T : IEvent;
    AlbionTask<TResult> RaiseQueryAsync<TResult>(IQueryEvent<TResult> e);
    void Enqueue(IEvent e);
#pragma warning restore CA1030

    void Distribute<T>(ICancellableEvent e, List<T> targets, Func<T, IComponent> projection);
    void On<T>(Action<T> callback) where T : IEvent;
    void OnAsync<T>(Func<T, AlbionTask> callback) where T : IEvent;
    void OnDirectCall<T>(Action<T> callback) where T : IEvent;
    void After<T>(Action<T> callback) where T : IEvent;
    void AfterAsync<T>(Func<T, AlbionTask> callback) where T : IEvent;
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
