using System;
using System.Collections.Generic;
using UAlbion.Api;

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
        static int _nesting;
        readonly IDictionary<Type, Handler> _handlers = new Dictionary<Type, Handler>();
        bool _isActive = true; // If false, then this component will not be attached to the exchange even if its parent is.
        bool _isSubscribed; // True if this component is currently attached to an event exchange

        /// <summary>
        /// The most recently attached event exchange, may currently be attached but
        /// not necessarily (e.g. if this component or any of its parents is inactive).
        /// Will be null until Attach has been called.
        /// </summary>
        protected EventExchange Exchange { get; private set; }

        /// <summary>
        /// The list of this components child components.
        /// The primary purpose of children is ensuring that the children are also attached and
        /// detached when the parent component is.
        /// </summary>
        protected IList<IComponent> Children { get; } = new List<IComponent>(); 

        /// <summary>
        /// Resolve the currently active object that provides the given interface.
        /// Service interfaces should only have a maximum of one active instance at any time.
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
        /// Enqueue an event with the currently subscribed event exchange to be raised
        /// after any currently processing synchronous events have completed.
        /// </summary>
        /// <param name="event"></param>
        protected void Enqueue(IEvent @event) => Exchange?.Enqueue(@event, this);

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
        protected void On<T>(Action<T> callback)
        {
            if (_handlers.ContainsKey(typeof(T)))
                return;

            var handler = new Handler<T>(callback, this);
            _handlers.Add(typeof(T), handler);
            if (_isSubscribed)
                Exchange.Subscribe(handler);
        }

        /// <summary>
        /// Cease handling the relevant event type.
        /// </summary>
        /// <typeparam name="T">The event type which should no longer be handled by this component.</typeparam>
        protected void Off<T>()
        {
            if (_handlers.Remove(typeof(T)) && _isSubscribed)
                Exchange.Unsubscribe<T>(this);
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
            if (_isActive)
                Exchange?.Attach(child);

            Children.Add(child);
            return child;
        }

        /// <summary>
        /// Remove all children of this component and detach them from the exchange.
        /// </summary>
        protected void RemoveAllChildren()
        {
            foreach (var child in Children)
                child.Detach();
            Children.Clear();
        }

        /// <summary>
        /// If the given component is a child of this component, detach it from the exchange and remove it from this components child list.
        /// </summary>
        /// <param name="child">The child component to remove</param>
        protected void RemoveChild(IComponent child)
        {
            int index = Children.IndexOf(child);
            if (index == -1) return;
            child.Detach();
            Children.RemoveAt(index);
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
            if (_isSubscribed)
                return;

            Exchange = exchange;

            if (!_isActive)
                return;

            _nesting++;
            Console.WriteLine("+".PadLeft(_nesting) + ToString());
            foreach (var child in Children)
                child.Attach(exchange);
            _nesting--;

            // exchange.Subscribe(null, this); // Ensure we always get added to the subscriber list, even if this component only uses subscription notifications.
            foreach (var kvp in _handlers)
                exchange.Subscribe(kvp.Value);
            _isSubscribed = true;
            Subscribed();
        }

        /// <summary>
        /// Detach the current component and its event handlers from any currently active event exchange.
        /// </summary>
        public void Detach()
        {
            if (Exchange == null)
                return;

            _nesting++;
            Console.WriteLine("-".PadLeft(_nesting) + ToString());
            Unsubscribed();

            foreach (var child in Children)
                child.Detach();

            _nesting--;
            Exchange.Unsubscribe(this);
            _isSubscribed = false;
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
            if (sender == this || Exchange == null)
                return;

            if (_handlers.TryGetValue(@event.GetType(), out var handler))
                handler.Invoke(@event);
        }
    }
}
