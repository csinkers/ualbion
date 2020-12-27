using System;
using System.Collections.Generic;
using System.Threading;
using UAlbion.Api;

#pragma warning disable CA1030 // Use events where appropriate
namespace UAlbion.Core
{
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
        static readonly Action<object> DummyContinuation = _ => { };
        static int _nesting;
        static int _nextId;
        readonly IDictionary<Type, Handler> _handlers = new Dictionary<Type, Handler>();
        readonly List<IComponent> _children = new List<IComponent>();
        bool _isActive = true; // If false, then this component will not be attached to the exchange even if its parent is.
        protected Component() => ComponentId = Interlocked.Increment(ref _nextId);

        /// <summary>
        /// Sequential id to uniquely identify a given component
        /// </summary>
        public int ComponentId { get; }

        /// <summary>
        /// True if this component is currently attached to an event exchange
        /// </summary>
        public bool IsSubscribed { get; private set; }

        /// <summary>
        /// The most recently attached event exchange, may currently be attached but
        /// not necessarily (e.g. if this component or any of its parents is inactive).
        /// Will be null until Attach has been called.
        /// </summary>
        protected EventExchange Exchange { get; private set; }

        /// <summary>
        /// The parent of this component, if it is a child.
        /// </summary>
        IComponent Parent { get; set; }

        /// <summary>
        /// The list of this component's child components.
        /// The primary purpose of children is ensuring that the children are also attached and
        /// detached when the parent component is.
        /// </summary>
        protected IReadOnlyList<IComponent> Children => _children;

        /// <summary>
        /// Resolve the currently active object that provides the given interface.
        /// Service interfaces should have a maximum of one active instance at any one time.
        /// </summary>
        /// <typeparam name="T">The interface type to resolve</typeparam>
        /// <returns></returns>
        protected T Resolve<T>() => Exchange.Resolve<T>();

        /// <summary>
        /// Raise an event via the currently subscribed event exchange (if subscribed), and
        /// distribute it to all components that have registered a handler.
        /// </summary>
        /// <param name="event">The event to raise</param>
        protected void Raise(IEvent @event) => Exchange?.Raise(@event, this);

        /// <summary>
        /// Raise an event via the currently subscribed event exchange (if subscribed), and
        /// distribute it to all components that have registered a handler. If any components
        /// have registered an async handler, they can call the continuation function to indicate
        /// they have completed handling the event.
        /// </summary>
        /// <param name="event">The event to raise</param>
        /// <param name="continuation">The continuation to be called by async handlers upon completion</param>
        /// <returns>The number of async handlers which have either already called the continuation or intend to call it in the future.</returns>
        protected int RaiseAsync(IAsyncEvent @event, Action continuation) => Exchange?.RaiseAsync(@event, this, continuation) ?? 0;

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
        protected int RaiseAsync<T>(IAsyncEvent<T> @event, Action<T> continuation) => Exchange?.RaiseAsync(@event, this, continuation) ?? 0;

        /// <summary>
        /// Enqueue an event with the currently subscribed event exchange to be raised
        /// after any currently processing synchronous events have completed.
        /// </summary>
        /// <param name="event"></param>
        protected void Enqueue(IEvent @event) => Exchange?.Enqueue(@event, this);

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

        /// <summary>
        /// Add an event handler callback to be called when the relevant event
        /// type is raised by other components.
        /// </summary>
        /// <typeparam name="T">The event type to handle</typeparam>
        /// <param name="callback">The function to call when the event is raised</param>
        protected void On<T>(Action<T> callback) where T : IEvent
        {
            if (_handlers.ContainsKey(typeof(T)))
                return;

            var handler = new Handler<T>(callback, this);
            _handlers.Add(typeof(T), handler);
            if (IsSubscribed)
                Exchange.Subscribe(handler);
        }

        /// <summary>
        /// Add an event handler callback to be called when the relevant event
        /// type is raised by other components.
        /// </summary>
        /// <typeparam name="T">The event type to handle</typeparam>
        /// <param name="callback">The function to call when the event is raised</param>
        protected void OnAsync<T>(Func<T, Action, bool> callback) where T : IAsyncEvent
        {
            if (_handlers.ContainsKey(typeof(T)))
                return;

            var handler = new AsyncHandler<T>(callback, this);
            _handlers.Add(typeof(T), handler);
            if (IsSubscribed)
                Exchange.Subscribe(handler);
        }

        /// <summary>
        /// Add an event handler callback to be called when the relevant event
        /// type is raised by other components.
        /// </summary>
        /// <typeparam name="TEvent">The event type to handle</typeparam>
        /// <typeparam name="TReturn">The type of value returned from async handlers for the event</typeparam>
        /// <param name="callback">The function to call when the event is raised</param>
        protected void OnAsync<TEvent, TReturn>(Func<TEvent, Action<TReturn>, bool> callback) where TEvent : IAsyncEvent<TReturn>
        {
            if (_handlers.ContainsKey(typeof(TEvent)))
                return;

            var handler = new AsyncHandler<TEvent, TReturn>(callback, this);
            _handlers.Add(typeof(TEvent), handler);
            if (IsSubscribed)
                Exchange.Subscribe(handler);
        }

        /// <summary>
        /// Cease handling the relevant event type.
        /// </summary>
        /// <typeparam name="T">The event type which should no longer be handled by this component.</typeparam>
        protected void Off<T>()
        {
            if (_handlers.Remove(typeof(T)) && IsSubscribed)
                Exchange.Unsubscribe<T>(this);
        }

        /// <summary>
        /// Whether the component is currently active. When a component becomes
        /// inactive all event handlers are removed from the exchange, all child
        /// components are also recursively removed from the exchange.
        /// </summary>
        public bool IsActive
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
            foreach (var kvp in _handlers)
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

            _children.Add(child);
            if (child is Component component)
                component.Parent = this;

            return child;
        }

        /// <summary>
        /// Remove all children of this component and detach them from the exchange.
        /// </summary>
        protected void RemoveAllChildren()
        {
            for (int i = Children.Count - 1; i >= 0; i--)
                Children[i].Remove(); // O(n²)… refactor if it ever becomes a problem.
        }

        /// <summary>
        /// If the given component is a child of this component, detach it from the
        /// exchange and remove it from this component's child list.
        /// </summary>
        /// <param name="child">The child component to remove</param>
        protected void RemoveChild(IComponent child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            int index = _children.IndexOf(child);
            if (index == -1) return;
            if (child is Component c)
                c.Parent = null;

            child.Remove();
            _children.RemoveAt(index);
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

            if (_handlers.TryGetValue(@event.GetType(), out var handler))
                handler.Invoke(@event, DummyContinuation);
        }
    }
}
#pragma warning restore CA1030 // Use events where appropriate
