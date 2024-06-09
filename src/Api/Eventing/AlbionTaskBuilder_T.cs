#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable CA1000 // Do not declare static members on generic types
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1822 // Mark members as static

namespace UAlbion.Api.Eventing;

[StructLayout(LayoutKind.Auto)]
public struct AlbionTaskBuilder<TResult>
{
    enum BuilderState
    {
        Indeterminate, // _core is null, _result has not been set
        Pending, // _core is set
        Complete, // _core is null, _result has been set
    }

    BuilderState _state;
    AlbionTaskCore<TResult>? _core;
    TResult _result;

    /// <summary>Gets the value task for this builder.</summary>
    public AlbionTask<TResult> Task => _state switch
    {
        BuilderState.Complete => new AlbionTask<TResult>(_result),
        BuilderState.Pending => new AlbionTask<TResult>(_core!),
        _ => throw new InvalidOperationException("Tried to get Task, but it hasn't been created")
    };

    /// <summary>Creates an instance of the <see cref="AlbionTaskBuilder{TResult}"/> struct.</summary>
    /// <returns>The initialized instance.</returns>
    public static AlbionTaskBuilder<TResult> Create() => default;

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
    /// <param name="result">The result to use to complete the value task.</param>
    public void SetResult(TResult result)
    {
        switch (_state)
        {
            case BuilderState.Indeterminate:
                _result = result;
                _state = BuilderState.Complete;
                break;

            case BuilderState.Pending:
                if (_core!.IsCompleted)
                    throw new InvalidOperationException("Tried to set value on a completed task");

                _core.SetResult(result);
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
                var box = new AlbionStateMachineBox<TResult, TStateMachine>();
                _core = box;
                _state = BuilderState.Pending;

                // In release mode, the state machine is a struct which actually includes this task builder.
                // By setting the state machine on the box, we're making a copy of the state of this task builder for when the continuation is called.
                // As a result, we need to make sure that we only perform the copy after any required changes to variables, e.g. setting _core and _state.
                box.StateMachine = stateMachine;
                awaiter.OnCompleted(box.MoveNextAction);
                break;
            }

            case BuilderState.Pending:
            {
                var box = (AlbionStateMachineBox<TResult, TStateMachine>)_core!;
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
                var box = new AlbionStateMachineBox<TResult, TStateMachine> { StateMachine = stateMachine };
                _core = box;
                _state = BuilderState.Pending;
                awaiter.UnsafeOnCompleted(box.MoveNextAction);
                break;
            }

            case BuilderState.Pending:
            {
                var box = (AlbionStateMachineBox<TResult, TStateMachine>)_core!;
                awaiter.UnsafeOnCompleted(box.MoveNextAction);
                break;
            }

            default:
                throw new InvalidOperationException("Called AwaitUnsafeOnCompleted on a completed AlbionTaskBuilder");
        }
    }

    sealed class AlbionStateMachineBox<TResult2, TStateMachine> : AlbionTaskCore<TResult2>
        where TStateMachine : IAsyncStateMachine
    {
        Action? _moveNextAction;
        public TStateMachine? StateMachine;
        public Action MoveNextAction => _moveNextAction ??= MoveNext;
#if DEBUG
        public AlbionStateMachineBox() : base($"ASMB<{typeof(TResult2)}, {typeof(TStateMachine)}>") { }
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