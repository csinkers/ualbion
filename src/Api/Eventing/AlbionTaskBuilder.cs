﻿#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1822 // Mark members as static

namespace UAlbion.Api.Eventing;

[StructLayout(LayoutKind.Auto)]
public struct AlbionTaskBuilder
{
    enum BuilderState
    {
        Indeterminate, // _core is null, _result has not been set
        Pending, // _core is set
        Complete, // _core is null, _result has been set
    }

    BuilderState _state;
    AlbionTaskCore<Unit>? _core;

    /// <summary>Gets the value task for this builder.</summary>
    public AlbionTask Task => _state switch
    {
        BuilderState.Complete => AlbionTask.CompletedTask,
        BuilderState.Pending => new AlbionTask(_core),
        _ => throw new InvalidOperationException("Tried to get Task, but it hasn't been created")
    };

    /// <summary>Creates an instance of the <see cref="AlbionTaskBuilder{TResult}"/> struct.</summary>
    /// <returns>The initialized instance.</returns>
    public static AlbionTaskBuilder Create() => default;

    /// <summary>Initiates the builder's execution with the associated state machine.</summary>
    /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
    /// <param name="stateMachine">The state machine instance, passed by reference.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        if (stateMachine == null) // TStateMachines are generally non-nullable value types, so this check will be elided
            throw new ArgumentNullException(nameof(stateMachine));

        stateMachine.MoveNext();
    }

    /// <summary>Marks the value task as successfully completed.</summary>
    public void SetResult()
    {
        switch (_state)
        {
            case BuilderState.Indeterminate:
                _state = BuilderState.Complete;
                break;

            case BuilderState.Pending:
                if (_core!.IsCompleted)
                    throw new InvalidOperationException("Tried to set value on a completed task");

                _core.SetResult(Unit.V);
                break;

            case BuilderState.Complete:
                throw new InvalidOperationException("Tried to set value on a completed task");
        }
    }

    /// <summary>Rethrows immediately, as AlbionTasks are only intended for single-threaded scenarios.</summary>
    /// <param name="exception">The exception to throw.</param>
    public void SetException(Exception exception) => throw exception;

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        /* Not used in .NET Core and later */
    }

    /// <summary>
    /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
    /// </summary>
    /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
    /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
    /// <param name="awaiter">The awaiter.</param>
    /// <param name="stateMachine">The state machine.</param>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        switch (_state)
        {
            case BuilderState.Indeterminate:
            {
                var box = new AlbionStateMachineBox<TStateMachine>();
                _core = box;
                _state = BuilderState.Pending;
                box.StateMachine = stateMachine;
                awaiter.OnCompleted(box.MoveNextAction);
                break;
            }

            case BuilderState.Pending:
                {
                    var box = (AlbionStateMachineBox<TStateMachine>)_core!;
                    awaiter.OnCompleted(box.MoveNextAction);
                    break;
                }

            default:
                throw new InvalidOperationException("Called AwaitOnCompleted on a completed AlbionTaskBuilder");
        }
    }

    /// <summary>
    /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
    /// </summary>
    /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
    /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
    /// <param name="awaiter">The awaiter.</param>
    /// <param name="stateMachine">The state machine.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        switch (_state)
        {
            case BuilderState.Indeterminate:
                {
                    var box = new AlbionStateMachineBox<TStateMachine> { StateMachine = stateMachine };
                    _core = box;
                    _state = BuilderState.Pending;
                    awaiter.UnsafeOnCompleted(box.MoveNextAction);
                    break;
                }

            case BuilderState.Pending:
                {
                    var box = (AlbionStateMachineBox<TStateMachine>)_core!;
                    awaiter.UnsafeOnCompleted(box.MoveNextAction);
                    break;
                }

            default:
                throw new InvalidOperationException("Called AwaitUnsafeOnCompleted on a completed AlbionTaskBuilder");
        }
    }

    sealed class AlbionStateMachineBox<TStateMachine> : AlbionTaskCore<Unit>
        where TStateMachine : IAsyncStateMachine
    {
        Action? _moveNextAction;
        public TStateMachine? StateMachine;
        public Action MoveNextAction => _moveNextAction ??= MoveNext;
#if DEBUG
        public AlbionStateMachineBox() : base($"ASMB<{typeof(TStateMachine)}>") { }
#endif
        void MoveNext()
        {
            Debug.Assert(StateMachine != null);
            StateMachine.MoveNext();

            if (IsCompleted)
                StateMachine = default;
        }
    }
}
