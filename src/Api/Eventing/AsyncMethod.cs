using System;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Defines the signature for an async event handler method using continuation passing style.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
/// <param name="e">The event to handle</param>
/// <param name="continuation">The continuation to be called upon asynchronous completion of the handler</param>
/// <returns>True if the continuation has been called synchronously during initial handling, or is going to be called in the future.
/// False if continuation will not be called.</returns>
public delegate bool AsyncMethod<in TEvent>(TEvent e, Action continuation) where TEvent : IAsyncEvent;

/// <summary>
/// Defines the signature for an async event handler method using continuation passing style.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
/// <typeparam name="TReturn">The return type for the event</typeparam>
/// <param name="e">The event to handle</param>
/// <param name="continuation">The continuation to be called upon asynchronous completion of the handler</param>
/// <returns>True if the continuation has been called synchronously during initial handling, or is going to be called in the future.
/// False if continuation will not be called.</returns>
public delegate bool AsyncMethod<in TEvent, out TReturn>(TEvent e, Action<TReturn> continuation) where TEvent : IAsyncEvent<TReturn>;

public delegate AlbionTask AlbionAsyncMethod<in TEvent>(TEvent e) where TEvent : IAsyncEvent;
public delegate AlbionTask<TResult> AlbionAsyncMethod<in TEvent, TResult>(TEvent e) where TEvent : IAsyncEvent<TResult>;