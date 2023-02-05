using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UAlbion.Api.Settings;

#pragma warning disable CA1030 // Use events where appropriate
namespace UAlbion.Api.Eventing;

/// <summary>
/// Component is effectively the base class of all game objects.
/// Each component belongs to a single EventExchange, and ideally communicates with
/// other components solely via sending and receiving events through its exchange.
///
/// In general, direct calls should only be used for read-only information retrieval, and
/// where possible should be done via Resolve to obtain an interface to game sub-systems.
/// Anything that modifies data, has side effects etc should be performed using an event.
///
/// This has benefits for tracing, reproducibility and modularity, and also provides a
/// convenient interface for console commands, key-bindings etc.
/// </summary>
public abstract class Component : IComponent
{
    public static bool TraceAttachment { get; set; }
    static readonly List<IComponent> EmptyChildren = new();
    static readonly ThreadLocal<object> ThreadContext = new();
    protected static object Context { get => ThreadContext.Value; set => ThreadContext.Value = value; }
    static int _nesting;
    static int _nextId;
    [DiagIgnore] List<IComponent> _children;
    [DiagIgnore] Dictionary<Type, Handler> _handlers;
    [DiagIgnore] bool _isActive = true; // If false, then this component will not be attached to the exchange even if its parent is.
    protected Component() => ComponentId = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Sequential id to uniquely identify a given component
    /// </summary>
    [DiagIgnore] public int ComponentId { get; }

    /// <summary>
    /// True if this component is currently attached to an event exchange
    /// </summary>
    [DiagIgnore] public bool IsSubscribed { get; private set; }

    /// <summary>
    /// The most recently attached event exchange, may currently be attached but
    /// not necessarily (e.g. if this component or any of its parents is inactive).
    /// Will be null until Attach has been called.
    /// </summary>
    [DiagIgnore] protected EventExchange Exchange { get; private set; }

    /// <summary>
    /// The parent of this component, if it is a child.
    /// </summary>
    public IComponent Parent { get; private set; }

    /// <summary>
    /// The list of this component's child components.
    /// The primary purpose of children is ensuring that the children are also attached and
    /// detached when the parent component is.
    /// This collection should only be modified by the AttachChild, RemoveChild and RemoveAllChildren methods.
    /// </summary>
    protected List<IComponent> Children => _children ?? EmptyChildren;

    /// <summary>
    /// Resolve the currently active object that provides the given interface.
    /// Service interfaces should have a maximum of one active instance at any one time.
    /// </summary>
    /// <typeparam name="T">The interface type to resolve</typeparam>
    /// <returns></returns>
    protected T Resolve<T>() => Exchange.Resolve<T>() ?? throw new MissingDependencyException($"{GetType().Name} could not locate dependency of type {typeof(T).Name}");

    /// <summary>
    /// Resolve the currently active object that provides the given interface.
    /// Service interfaces should have a maximum of one active instance at any one time.
    /// </summary>
    /// <typeparam name="T">The interface type to resolve</typeparam>
    /// <returns></returns>
    protected T TryResolve<T>() => Exchange.Resolve<T>();

    /// <summary>
    /// Raise an event via the currently subscribed event exchange (if subscribed), and
    /// distribute it to all components that have registered a handler.
    /// </summary>
    /// <param name="event">The event to raise</param>
    protected void Raise<T>(T @event) where T : IEvent => Exchange?.Raise(@event, this);

    /// <summary>
    /// Raise an event via the currently subscribed event exchange (if subscribed), and
    /// distribute it to all components that have registered a handler. If any components
    /// have registered an async handler, they can call the continuation function to indicate
    /// they have completed handling the event.
    /// </summary>
    /// <param name="event">The event to raise</param>
    /// <param name="continuation">The continuation to be called by async handlers upon completion</param>
    /// <returns>The number of async handlers which have either already called the continuation or intend to call it in the future.</returns>
    protected int RaiseAsync(IAsyncEvent @event, Action continuation)
    {
        if (continuation == null) throw new ArgumentNullException(nameof(continuation));
        var context = Context;
        int promisedCalls = Exchange?.RaiseAsync(@event, this, 
            () =>
            {
                Context = context;
                continuation();
            }) ?? 0;

        Context = context;
        return promisedCalls;
    }

    /// <summary>
    /// Raise an event via the currently subscribed event exchange (if subscribed), and
    /// distribute it to all components that have registered a handler. If any components
    /// have registered an async handler, they can call the continuation function to indicate
    /// they have completed handling the event.
    /// </summary>
    /// <typeparam name="T">The return value that async handlers should supply upon completion.</typeparam>
    /// <param name="event">The event to raise</param>
    /// <param name="continuation">The continuation to be called by async handlers upon completion</param>
    /// <returns>The number of async handlers which have either already called the continuation or intend to call it in the future.</returns>
    protected int RaiseAsync<T>(IAsyncEvent<T> @event, Action<T> continuation)
    {
        if (continuation == null) throw new ArgumentNullException(nameof(continuation));
        var context = Context;
        int promisedCalls = Exchange?.RaiseAsync(@event, this,
            x =>
            {
                Context = context;
                continuation(x);
            }) ?? 0;

        Context = context;
        return promisedCalls;
    }

    /// <summary>
    /// Enqueue an event with the currently subscribed event exchange to be raised
    /// after any currently processing synchronous events have completed.
    /// </summary>
    /// <param name="event"></param>
    protected void Enqueue(IEvent @event) => Exchange?.Enqueue(@event, this);

    /// <summary>
    /// Distribute a cancellable event to each target in turn until the event is cancelled.
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <param name="event">The event to distribute</param>
    /// <param name="targets">The targets to distribute the event to</param>
    /// <param name="projection">A function that maps from the target type to its associated component which will receive the event</param>
    /// <exception cref="ArgumentNullException"></exception>
    protected void Distribute<T>(ICancellableEvent @event, IEnumerable<T> targets, Func<T, IComponent> projection)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (targets == null) throw new ArgumentNullException(nameof(targets));

        @event.Propagating = true;
        foreach (var target in targets)
        {
            if (!@event.Propagating) break;
            var component = projection(target);
            component?.Receive(@event, this);
        }
    }

    protected virtual void Subscribing() { }

    /// <summary>
    /// Called when the component is attached / subscribed to an event exchange.
    /// Does nothing by default, but is provided for individual component implementations
    /// to override.
    /// </summary>
    protected virtual void Subscribed() { }

    /// <summary>
    /// Called when the component is detached / unsubscribed from an event exchange.
    /// Does nothing by default, but is provided for individual component implementations
    /// to override.
    /// </summary>
    protected virtual void Unsubscribed() { }

    void OnCore<T, TCallback>(TCallback callback, Func<TCallback, Component, Handler> handlerConstructor)
    {
        _handlers ??= new Dictionary<Type, Handler>();
        _handlers.TryGetValue(typeof(T), out var handler);

        if (handler == null)
        {
            handler = handlerConstructor(callback, this);
            _handlers.Add(typeof(T), handler);
        }

        if (handler.IsActive)
            return;

        if (IsSubscribed)
            Exchange.Subscribe(handler);

        handler.IsActive = true;
    }

    /// <summary>
    /// Add an event handler callback to be called when the relevant event
    /// type is raised by other components.
    /// </summary>
    /// <typeparam name="T">The event type to handle</typeparam>
    /// <param name="callback">The function to call when the event is raised</param>
    protected void On<T>(Action<T> callback) where T : IEvent =>
        OnCore<T, Action<T>>(callback, (c, x) => new Handler<T>(c, x, false));

    /// <summary>
    /// Add an event handler callback to be called when the relevant event
    /// type is raised by other components.
    /// </summary>
    /// <typeparam name="T">The event type to handle</typeparam>
    /// <param name="callback">The function to call when the event is raised</param>
    protected void OnAsync<T>(AsyncMethod<T> callback) where T : IAsyncEvent =>
        OnCore<T, AsyncMethod<T>>(callback, (c, x) => new AsyncHandler<T>(c, x, false));

    /// <summary>
    /// Add an event handler callback to be called when the relevant event
    /// type is raised by other components.
    /// </summary>
    /// <typeparam name="TEvent">The event type to handle</typeparam>
    /// <typeparam name="TReturn">The type of value returned from async handlers for the event</typeparam>
    /// <param name="callback">The function to call when the event is raised</param>
    protected void OnAsync<TEvent, TReturn>(AsyncMethod<TEvent, TReturn> callback) where TEvent : IAsyncEvent<TReturn> =>
        OnCore<TEvent, AsyncMethod<TEvent, TReturn>>(
            callback, 
            (c,x) => new AsyncHandler<TEvent, TReturn>(c, x, false));

    /// <summary>
    /// Add an event handler callback to be called only when the relevant event
    /// type is passed directly to the Receive method (i.e. events raised through the
    /// exchange will not cause the handler to be called)
    /// </summary>
    /// <typeparam name="T">The event type to handle</typeparam>
    /// <param name="callback">The function to call when the event is raised</param>
    protected void OnDirectCall<T>(Action<T> callback) where T : IEvent => 
        OnCore<T, Action<T>>(callback, (c, x) => new ReceiveOnlyHandler<T>(c, x));

    /// <summary>
    /// Add an event handler callback to be called after all the On handlers for
    /// the relevant event type have been called.
    /// </summary>
    /// <typeparam name="T">The event type to handle</typeparam>
    /// <param name="callback">The function to call after the event is raised</param>
    protected void After<T>(Action<T> callback) where T : IEvent => 
        OnCore<T, Action<T>>(callback, (c, x) => new Handler<T>(c, x, true));

    /// <summary>
    /// Add an event handler callback to be called after all the On handlers for
    /// the relevant event type have been called.
    /// </summary>
    /// <typeparam name="T">The event type to handle</typeparam>
    /// <param name="callback">The function to call when the event is raised</param>
    protected void AfterAsync<T>(AsyncMethod<T> callback) where T : IAsyncEvent =>
        OnCore<T, AsyncMethod<T>>(callback, (c,x)=> new AsyncHandler<T>(c, x, true));

    /// <summary>
    /// Add an event handler callback to be called after all the On handlers for
    /// the relevant event type have been called.
    /// </summary>
    /// <typeparam name="TEvent">The event type to handle</typeparam>
    /// <typeparam name="TReturn">The type of value returned from async handlers for the event</typeparam>
    /// <param name="callback">The function to call when the event is raised</param>
    protected void AfterAsync<TEvent, TReturn>(AsyncMethod<TEvent, TReturn> callback) where TEvent : IAsyncEvent<TReturn> =>
        OnCore<TEvent, AsyncMethod<TEvent, TReturn>>(
            callback, 
            (c,x) => new AsyncHandler<TEvent, TReturn>(c, x, true));

    /// <summary>
    /// Cease handling the relevant event type.
    /// </summary>
    /// <typeparam name="T">The event type which should no longer be handled by this component.</typeparam>
    protected void Off<T>()
    {
        if (_handlers?.TryGetValue(typeof(T), out var handler) != true)
            return;

        handler.IsActive = false;
        if (IsSubscribed)
            Exchange.Unsubscribe<T>(this);
    }

    /// <summary>
    /// Whether the component is currently active. When a component becomes
    /// inactive all event handlers are removed from the exchange, all child
    /// components are also recursively removed from the exchange.
    /// </summary>
    [DiagIgnore] public bool IsActive
    {
        get => _isActive;
        set
        {
            if (value == _isActive)
                return;

            _isActive = value;

            if (value) Attach(Exchange);
            else Detach();
        }
    }

    /// <summary>
    /// Attach this component to the given exchange and register all event
    /// handlers that have been configured using On.
    /// </summary>
    /// <param name="exchange">The event exchange that this component should be attached to</param>
    public void Attach(EventExchange exchange)
    {
        if (exchange == null) throw new ArgumentNullException(nameof(exchange));
        if (IsSubscribed)
            return;

        Exchange = exchange;

        if (!_isActive)
            return;

        Subscribing();
        _nesting++;
        if (TraceAttachment)
            Console.WriteLine("+".PadLeft(_nesting) + ToString());

        foreach (var child in Children)
            child.Attach(exchange);

        _nesting--;

        // exchange.Subscribe(null, this); // Ensure we always get added to the subscriber list, even if this component only uses subscription notifications.
        if (_handlers != null)
            foreach (var kvp in _handlers)
                if (kvp.Value.IsActive)
                    exchange.Subscribe(kvp.Value);

        IsSubscribed = true;
        Subscribed();
    }

    /// <summary>
    /// Detach the current component and its event handlers from any currently active event exchange.
    /// </summary>
    protected void Detach()
    {
        if (Exchange == null)
            return;

        _nesting++;
        if (TraceAttachment)
            Console.WriteLine("-".PadLeft(_nesting) + ToString());

        Unsubscribed();

        foreach (var child in Children)
            if (child is Component c)
                c.Detach();

        _nesting--;
        Exchange.Unsubscribe(this);
        IsSubscribed = false;
    }

    /// <summary>
    /// Remove this component from the event system
    /// </summary>
    public void Remove()
    {
        Detach();
        Exchange = null;

        if (Parent is Component parent)
        {
            Parent = null;
            parent.RemoveChild(this);
        }
    }

    /// <summary>
    /// Add a component to the collection of this component's children, will attach the
    /// component to this component's exchange if this component is currently attached.
    /// </summary>
    /// <typeparam name="T">The type of the child component</typeparam>
    /// <param name="child">The component to add</param>
    /// <returns>The child is also returned to allow constructs like _localVar = AttachChild(new SomeType());</returns>
    protected T AttachChild<T>(T child) where T : IComponent
    {
        // For situations like AttachChild(someParameterPassedAsInterface as IComponent)
        // we don't need to do anything if the implementation of the interface isn't actually a component. 
        if (child == null) 
            return default;

        if (_isActive) // Children will be attached when this component is made active.
            Exchange?.Attach(child);

        _children ??= new List<IComponent>();
        _children.Add(child);
        if (child is Component component)
        {
#if DEBUG
            if (component.Parent != this && component.Parent != null)
                throw new InvalidOperationException($"Attempted to attach {component} to {this}, but it is already attached to {component.Parent}");
#endif
            component.Parent = this;
        }

        return child;
    }

    /// <summary>
    /// Remove all children of this component and detach them from the exchange.
    /// </summary>
    protected void RemoveAllChildren()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
            Children[i].Remove(); // O(n²)… refactor if it ever becomes a problem.
        _children = null;
    }

    /// <summary>
    /// If the given component is a child of this component, detach it from the
    /// exchange and remove it from this component's child list.
    /// </summary>
    /// <param name="child">The child component to remove</param>
    protected void RemoveChild(IComponent child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (_children == null) return;
        int index = _children.IndexOf(child);
        if (index == -1) return;
        if (child is Component c)
            c.Parent = null;

        child.Remove();
        _children.RemoveAt(index);
        if (_children.Count == 0)
            _children = null;
    }

    /// <summary>
    /// Invoke any active handler for the given event. Mostly used for direct
    /// component-component invocations (rare), event exchange handlers
    /// typically bypass this step and call into the handler function directly.
    /// </summary>
    /// <param name="event">The event being raised</param>
    /// <param name="sender">The component which generated the event</param>
    public void Receive(IEvent @event, object sender)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (sender == this || !IsSubscribed || Exchange == null)
            return;

        if (_handlers != null && _handlers.TryGetValue(@event.GetType(), out var handler) && handler.IsActive)
            handler.Invoke(@event, DummyContinuation.Instance);
    }

    /// <summary>
    /// Gets the current value for an IVar
    /// </summary>
    /// <typeparam name="T">The value type of the IVar</typeparam>
    /// <param name="varInfo">The IVar</param>
    /// <returns>The current value</returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected T Var<T>(IVar<T> varInfo)
    {
        if (varInfo == null) throw new ArgumentNullException(nameof(varInfo));
        var varSet = Resolve<IVarSet>();
        return varInfo.Read(varSet);
    }

    // Logging helpers
    protected void Verbose(string msg, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        => Raise(new LogEvent(LogLevel.Verbose, msg, file, member, line));
    protected void Info(string msg, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        => Raise(new LogEvent(LogLevel.Info, msg, file, member, line));
    protected void Warn(string msg, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        => Raise(new LogEvent(LogLevel.Warning, msg, file, member, line));
    protected void Error(string msg, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        => Raise(new LogEvent(LogLevel.Error, msg, file, member, line));
    protected void Critical(string msg, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        => Raise(new LogEvent(LogLevel.Critical, msg, file, member, line));
}

#pragma warning restore CA1030 // Use events where appropriate