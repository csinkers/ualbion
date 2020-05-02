﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Core.Events;

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
        // Used when null is passed to the constructor so we
        // won't need null checks when accessing _handlers.
        protected static readonly HandlerSet EmptyHandlerSet = new HandlerSet();

        protected abstract class Handler
        {
            public Type Type { get; }
            protected Handler(Type type) { Type = type; }
            public abstract void Invoke(Component instance, IEvent @event);
        }

        protected class Handler<TInstance, TEvent> : Handler where TInstance : Component
        {
            readonly Action<TInstance, TEvent> _callback;
            public Handler(Action<TInstance, TEvent> callback) : base(typeof(TEvent)) => _callback = callback;
            public override void Invoke(Component instance, IEvent @event) => _callback((TInstance)instance, (TEvent)@event);
        }

        protected class HandlerSet : Dictionary<Type, Handler> // This class is essentially some syntactic sugar for declaring handler dictionaries.
        {
            public HandlerSet(HandlerSet parent, params Handler[] handlers)
            {
                foreach (var handler in parent)
                    Add(handler.Key, handler.Value);

                if (handlers == null)
                    return;

                foreach (var handler in handlers)
                    this[handler.Type] = handler;
            }

            public HandlerSet(params Handler[] handlers)
            {
                if (handlers == null)
                    return;
                foreach (var handler in handlers)
                    Add(handler.Type, handler);
            }
        }

        // Helper to reduce verbosity
        protected static Handler H<TComponent, TEvent>(Action<TComponent, TEvent> callback) where TComponent : Component
            => new Handler<TComponent, TEvent>(callback);

        // Usually set to a static per-type HandlerSet, but can also be per-instance is so desired.
        readonly IDictionary<Type, Handler> _handlers;
        bool _isActive = true;
        bool _isAttached;
        protected EventExchange Exchange { get; private set; } // N.B. will be null until subscribed.
        protected IList<IComponent> Children { get; } = new List<IComponent>(); // Primary purpose of children is ensuring that the children are also detached when the parent component is.
        protected Component() : this(null) { }
        protected Component(IDictionary<Type, Handler> handlers) => _handlers = handlers ?? EmptyHandlerSet;

        protected T AttachChild<T>(T child) where T : IComponent
        {
            if (_isActive)
                Exchange?.Attach(child);
            Children.Add(child);
            return child;
        }

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

        public void Attach(EventExchange exchange)
        {
            if (_isAttached)
                return;

            Exchange = exchange;

            if (!_isActive)
                return;

            foreach (var child in Children)
                child.Attach(exchange);

            exchange.Subscribe(null, this); // Ensure we always get added to the subscriber list, even if this component only uses subscription notifications.
            foreach (var kvp in _handlers)
                exchange.Subscribe(kvp.Key, this);
            _isAttached = true;
            Subscribed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Resolve<T>() => Exchange.Resolve<T>(); // Convenience method to save a bit of typing

        public void Receive(IEvent @event, object sender)
        {
            if (sender == this || Exchange == null)
                return;

            if (_handlers.TryGetValue(@event.GetType(), out var handler))
                handler.Invoke(this, @event);
        }

        public virtual void Detach()
        {
            if (Exchange == null)
                return;

            foreach (var child in Children)
                child.Detach();

            Exchange.Unsubscribe(this);
            _isAttached = false;
        }

        protected virtual void Subscribed() { }
        public bool IsSubscribed => _isAttached;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Raise(IEvent @event) => Exchange?.Raise(@event, this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Enqueue(IEvent @event) => Exchange?.Enqueue(@event, this);
    }
}
