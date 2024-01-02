using System;

namespace UAlbion.Api.Eventing;

public abstract class Handler
{
    public abstract bool ShouldSubscribe { get; }
    public bool IsActive { get; set; }
    public bool IsPostHandler { get; }
    public Type Type { get; }
    public IComponent Component { get; }
    protected Handler(Type type, IComponent component, bool isPostHandler)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Component = component ?? throw new ArgumentNullException(nameof(component));
        IsPostHandler = isPostHandler;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns>True if the handler intends to call, or has already called the continuation.</returns>
    public abstract void Invoke(IEvent e);
    public override string ToString() => $"H<{Component.GetType().Name}, {Type.Name}>";
}

public class Handler<TEvent> : Handler where TEvent : IEvent
{
    public override bool ShouldSubscribe => true;
    Action<TEvent> Callback { get; }
    public Handler(Action<TEvent> callback, IComponent component, bool isPostHandler) 
        : base(typeof(TEvent), component, isPostHandler) => Callback = callback;
    public override void Invoke(IEvent e) => Callback((TEvent)e);
    public override AlbionTask InvokeAsync(IAsyncEvent e)
    {
        Callback((TEvent)e);
        return AlbionTask.CompletedTask;
    }
}

public class ReceiveOnlyHandler<TEvent> : Handler where TEvent : IEvent
{
    public override bool ShouldSubscribe => false;
    Action<TEvent> Callback { get; }
    public ReceiveOnlyHandler(Action<TEvent> callback, IComponent component) 
        : base(typeof(TEvent), component, false) => Callback = callback;
    public override bool Invoke(IEvent e, object continuation) { Callback((TEvent) e); return false; }
}

public class AsyncHandler<TEvent> : Handler where TEvent : IAsyncEvent
{
    public override bool ShouldSubscribe => true;
    AsyncMethod<TEvent> Callback { get; }
    public AsyncHandler(AsyncMethod<TEvent> callback, IComponent component, bool isPostHandler)
        : base(typeof(TEvent), component, isPostHandler) => Callback = callback;
    public override bool Invoke(IEvent e, object continuation) => Callback((TEvent)e, (Action)continuation ?? DummyContinuation.Instance);
}

public class AsyncHandler<TEvent, TReturn> : Handler where TEvent : IAsyncEvent<TReturn>
{
    public override bool ShouldSubscribe => true;
    AsyncMethod<TEvent, TReturn> Callback { get; }
    public AsyncHandler(AsyncMethod<TEvent, TReturn> callback, IComponent component, bool isPostHandler)
        : base(typeof(TEvent), component, isPostHandler) => Callback = callback;
    public override bool Invoke(IEvent e, object continuation) 
        => Callback((TEvent)e, (Action<TReturn>)continuation ?? DummyContinuation<TReturn>.Instance);
}

public abstract class AsyncHandler2<TResult> : Handler
{
    public override bool ShouldSubscribe => true;
    protected AsyncHandler2(Type type, IComponent component, bool isPostHandler) : base(type, component, isPostHandler) { }
    public abstract AlbionTask<TResult> InvokeAsync(IEvent e);
    public override void Invoke(IEvent e) => throw new NotSupportedException();
}

public class AsyncHandler2<TEvent, TResult> : AsyncHandler2<TResult> where TEvent : IAsyncEvent<TResult>
{
    AlbionAsyncMethod<TEvent, TResult> Callback { get; }
    public AsyncHandler2(AlbionAsyncMethod<TEvent, TResult> callback, IComponent component, bool isPostHandler)
        : base(typeof(TEvent), component, isPostHandler) => Callback = callback;
    public override AlbionTask<TResult> InvokeAsync(IEvent e) => Callback((TEvent)e);
}

