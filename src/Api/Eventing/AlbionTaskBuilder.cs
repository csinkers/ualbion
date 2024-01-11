#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1000 // Do not declare static members on generic types
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1822 // Mark members as static

namespace UAlbion.Api.Eventing;

[StructLayout(LayoutKind.Auto)]
public struct AlbionTaskBuilder<TResult>
{
    AlbionTaskCore<TResult>? _core;
    TResult _result;

    /// <summary>Sentinel object used to indicate that the builder completed synchronously and successfully.</summary>
    /// <remarks>
    /// To avoid memory safety issues even in the face of invalid race conditions, we ensure that the type of this object
    /// is valid for the mode in which we're operating.  As such, it's cached on the generic builder per TResult
    /// rather than having one sentinel instance for all types.
    /// </remarks>
    static readonly SentinelTaskCore<TResult> s_syncSuccessSentinel = new();

    /// <summary>Gets the value task for this builder.</summary>
    public AlbionTask<TResult> Task => _core == s_syncSuccessSentinel
        ? new AlbionTask<TResult>(_result)
        : new AlbionTask<TResult>(_core ?? throw new InvalidOperationException("Tried to get Task, but it hasn't been created"));

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
        if (_core is null)
        {
            _result = result;
            _core = s_syncSuccessSentinel;
        }
        else
        {
            if (_core.IsCompleted)
                throw new InvalidOperationException("Tried to set value on a completed task");

            _core.SetResult(result);
        }
    }

    /// <summary>Rethrows immediately, as AlbionTasks are only intended for single-threaded scenarios.</summary>
    /// <param name="exception">The exception to throw.</param>
    public void SetException(Exception exception) => throw exception;
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }

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
        awaiter.OnCompleted(GetStateMachineBox(ref stateMachine, ref _core).MoveNextAction);
    }

    static AlbionStateMachineBox<TResult2, TStateMachine> GetStateMachineBox<TResult2, TStateMachine>(
        ref TStateMachine stateMachine,
        ref AlbionTaskCore<TResult2> core)
            where TStateMachine : IAsyncStateMachine
    {
        // Check first for the most common case: not the first yield in an async method.
        // In this case, the first yield will have already "boxed" the state machine in
        // a strongly-typed manner into an AsyncStateMachineBox.  It will already contain
        // the state machine as well as a MoveNextDelegate and a context.  The only thing
        // we might need to do is update the context if that's changed since it was stored.
        if (core is AlbionStateMachineBox<TResult2, TStateMachine> stronglyTypedBox)
            return stronglyTypedBox;

        var box = new AlbionStateMachineBox<TResult2, TStateMachine>();
        core = box;
        box._stateMachine = stateMachine;
        return box;
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
        var box = GetStateMachineBox(ref stateMachine, ref _core);
        awaiter.UnsafeOnCompleted(box.MoveNextAction);
    }

    class AlbionStateMachineBox<TResult2, TStateMachine> : AlbionTaskCore<TResult2>
        where TStateMachine : IAsyncStateMachine
    {
        Action? _moveNextAction;
        public TStateMachine? _stateMachine;
        public Action MoveNextAction => _moveNextAction ??= MoveNext;
#if DEBUG
        public AlbionStateMachineBox() : base($"ASMB<{typeof(TResult2)}, {typeof(TStateMachine)}>") { }
#endif
        void MoveNext()
        {
            Debug.Assert(_stateMachine != null);
            _stateMachine.MoveNext();

            if (IsCompleted)
                _stateMachine = default;
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public struct AlbionTaskBuilder
{
    AlbionTaskCore<Unit>? _core;

    /// <summary>Sentinel object used to indicate that the builder completed synchronously and successfully.</summary>
    /// <remarks>
    /// To avoid memory safety issues even in the face of invalid race conditions, we ensure that the type of this object
    /// is valid for the mode in which we're operating.  As such, it's cached on the generic builder per TResult
    /// rather than having one sentinel instance for all types.
    /// </remarks>
    static readonly AlbionTaskCore<Unit> s_syncSuccessSentinel = new();

    /// <summary>Gets the value task for this builder.</summary>
    public AlbionTask Task => new(_core ?? throw new InvalidOperationException("Tried to get Task, but it hasn't been created"));

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
        if (_core is null)
        {
            _core = s_syncSuccessSentinel;
        }
        else
        {
            if (_core.IsCompleted)
                throw new InvalidOperationException("Tried to set value on a completed task");

            _core.SetResult(Unit.V);
        }
    }

    /// <summary>Rethrows immediately, as AlbionTasks are only intended for single-threaded scenarios.</summary>
    /// <param name="exception">The exception to throw.</param>
    public void SetException(Exception exception) => throw exception;
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }

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
        awaiter.OnCompleted(GetStateMachineBox(ref stateMachine, ref _core).MoveNextAction);
    }

    static AlbionStateMachineBox<TResult2, TStateMachine> GetStateMachineBox<TResult2, TStateMachine>(
        ref TStateMachine stateMachine,
        ref AlbionTaskCore<TResult2> core)
            where TStateMachine : IAsyncStateMachine
    {
        // Check first for the most common case: not the first yield in an async method.
        // In this case, the first yield will have already "boxed" the state machine in
        // a strongly-typed manner into an AsyncStateMachineBox.  It will already contain
        // the state machine as well as a MoveNextDelegate and a context.  The only thing
        // we might need to do is update the context if that's changed since it was stored.
        if (core is AlbionStateMachineBox<TResult2, TStateMachine> stronglyTypedBox)
            return stronglyTypedBox;

        var box = new AlbionStateMachineBox<TResult2, TStateMachine>();
        core = box;
        box._stateMachine = stateMachine;
        return box;
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
        var box = GetStateMachineBox(ref stateMachine, ref _core);
        awaiter.UnsafeOnCompleted(box.MoveNextAction);
    }

    class AlbionStateMachineBox<TResult2, TStateMachine> : AlbionTaskCore<TResult2>
        where TStateMachine : IAsyncStateMachine
    {
        Action? _moveNextAction;
        public TStateMachine? _stateMachine;
        public Action MoveNextAction => _moveNextAction ??= MoveNext;
#if DEBUG
        public AlbionStateMachineBox() : base($"ASMB<{typeof(TResult2)}, {typeof(TStateMachine)}>") { }
#endif
        void MoveNext()
        {
            Debug.Assert(_stateMachine != null);
            _stateMachine.MoveNext();

            if (IsCompleted)
                _stateMachine = default;
        }
    }
}

#pragma warning restore CA1822 // Mark members as static
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1000 // Do not declare static members on generic types