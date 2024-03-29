﻿using System;
using System.Collections.Generic;
using UAlbion.Api.Settings;

namespace UAlbion.Api.Eventing;

public class AdHocComponent : Component
{
    public string Name { get; }
    public override string ToString() => $"AdHoc:{Name}";

    public AdHocComponent(string name, Action<IAdHocComponentHelper> constructor)
    {
        if (constructor == null) throw new ArgumentNullException(nameof(constructor));
        Name = name;
        constructor(new Helper(this));
    }

    public static AdHocComponent Build<T>(string name, T context, Action<T, IAdHocComponentHelper> constructor)
        => new(name, x => constructor(context, x));

    public Action OnSubscribing { get; init; }
    public Action OnSubscribed { get; init; }
    public Action OnUnsubscribed { get; init; }

    protected override void Subscribed() => OnSubscribed?.Invoke();
    protected override void Subscribing() => OnSubscribing?.Invoke();
    protected override void Unsubscribed() => OnUnsubscribed?.Invoke();

    class Helper : IAdHocComponentHelper
    {
        readonly AdHocComponent _this;
        public Helper(AdHocComponent adhoc) => _this = adhoc;
        public object Context => Component.Context;
        public EventExchange Exchange => _this.Exchange;
        public IReadOnlyList<IComponent> Children => _this.Children;
        public T Resolve<T>() => _this.Resolve<T>();
        public T TryResolve<T>() => _this.TryResolve<T>();
        public void Raise<T>(T @event) where T : IEvent => _this.Raise(@event);
        public int RaiseAsync(IAsyncEvent @event, Action continuation) => _this.RaiseAsync(@event, continuation);
        public int RaiseAsync<T>(IAsyncEvent<T> @event, Action<T> continuation) => _this.RaiseAsync(@event, continuation);
        public void Enqueue(IEvent @event) => _this.Enqueue(@event);
        public void Distribute<T>(ICancellableEvent @event, IEnumerable<T> targets, Func<T, IComponent> projection) => _this.Distribute(@event, targets, projection);
        public void On<T>(Action<T> callback) where T : IEvent => _this.On(callback);
        public void OnAsync<T>(AsyncMethod<T> callback) where T : IAsyncEvent => _this.OnAsync(callback);
        public void OnAsync<TEvent, TReturn>(AsyncMethod<TEvent, TReturn> callback) where TEvent : IAsyncEvent<TReturn> => _this.OnAsync(callback);
        public void OnDirectCall<T>(Action<T> callback) where T : IEvent => _this.OnDirectCall(callback);
        public void After<T>(Action<T> callback) where T : IEvent => _this.After(callback);
        public void AfterAsync<T>(AsyncMethod<T> callback) where T : IAsyncEvent => _this.AfterAsync(callback);
        public void AfterAsync<TEvent, TReturn>(AsyncMethod<TEvent, TReturn> callback) where TEvent : IAsyncEvent<TReturn> => _this.AfterAsync(callback);
        public void Off<T>() => _this.Off<T>();
        public void Detach() => _this.Detach();
        public T AttachChild<T>(T child) where T : IComponent => _this.AttachChild(child);
        public void RemoveAllChildren() => _this.RemoveAllChildren();
        public void RemoveChild(IComponent child) => _this.RemoveChild(child);
        public T Var<T>(IVar<T> varInfo) => _this.Var<T>(varInfo);
        public void Verbose(string msg) => _this.Verbose(msg);
        public void Info(string msg) => _this.Info(msg);
        public void Warn(string msg) => _this.Warn(msg);
        public void Error(string msg) => _this.Error(msg);
        public void Critical(string msg) => _this.Critical(msg);
    }
}
