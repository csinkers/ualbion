#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UAlbion.Api.Eventing;

/*

States:
  Uninitialised _core null, _result default
  Complete _core null, _result set
  Pending _core set, _result default
 */

/// <summary>
/// A much simpler alternative to Task. Handles all async on the main thread,
/// each frame any queued unblocked AlbionTasks will be progressed. Tasks that
/// do not return a value should use the Unit type.
/// </summary>
[AsyncMethodBuilder(typeof(AlbionTaskBuilder<>))]
public readonly struct AlbionTask<T> : INotifyCompletion, IEquatable<AlbionTask<T>>
{
    readonly AlbionTaskCore<T>? _core;
    readonly T? _result;

    public bool IsValid { get; }

    /// <summary>
    /// Default task in an invalid state, cannot be awaited etc
    /// </summary>
    public AlbionTask()
    {
        IsValid = false;
        _core = null;
        _result = default;
    }

    /// <summary>
    /// Create a completed task
    /// </summary>
    public AlbionTask(T value)
    {
        IsValid = true;
        _core = null;
        _result = value;
    }

    /// <summary>
    /// Create a task that will complete later
    /// </summary>
    internal AlbionTask(AlbionTaskCore<T> core)
    {
        IsValid = true;
        _core = core;
        _result = default;
    }

    public AlbionTask<T> GetAwaiter() => this;
    public bool IsCompleted => _core == null || _core.IsCompleted;
    public void OnCompleted(Action continuation)
    {
        if (!IsValid) throw new InvalidOperationException("Tried to add a continuation to an invalid task");
        if (_core == null) throw new InvalidOperationException("Tried to add a continuation to a completed task");

        _core.OnCompleted(continuation);
    }

    public T GetResult() => _core == null ? _result! : _core.GetResult();
    internal void SetResult(T value)
    {
        if (!IsValid) throw new InvalidOperationException("Tried to set result of an invalid task");
        if (_core == null) throw new InvalidOperationException("Tried to set result of a completed task");
        _core.SetResult(value);
    }

    public bool Equals(AlbionTask<T> other) => IsValid == other.IsValid && Equals(_core, other._core) && EqualityComparer<T?>.Default.Equals(_result, other._result);
    public override bool Equals(object? obj) => obj is AlbionTask<T> other && Equals(other);
    public static bool operator ==(AlbionTask<T> x, AlbionTask<T> y) => x.Equals(y);
    public static bool operator !=(AlbionTask<T> x, AlbionTask<T> y) => !(x == y);
    public override int GetHashCode() => HashCode.Combine(IsValid, _core, _result);
}

[AsyncMethodBuilder(typeof(AlbionTaskBuilder))]
public readonly struct AlbionTask : INotifyCompletion, IEquatable<AlbionTask>
{
    readonly AlbionTaskCore<Unit>? _core;

    public bool IsValid { get; }

    public static AlbionTask CompletedTask { get; } = new(null);
    public AlbionTask<Unit> UnitTask => _core == null 
        ? new AlbionTask<Unit>(Unit.V) 
        : new AlbionTask<Unit>(_core);

    internal AlbionTask(AlbionTaskCore<Unit>? core)
    {
        IsValid = true;
        _core = core;
    }

    public AlbionTask()
    {
        IsValid = false;
        _core = null;
    }

    public AlbionTask GetAwaiter() => this;
    public bool IsCompleted => _core == null || _core.IsCompleted;
    public void OnCompleted(Action continuation)
    {
        if (_core == null)
            throw new InvalidOperationException("Tried to add a continuation to a completed task");

        _core.OnCompleted(continuation);
    }

    public void GetResult()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Tried to get result of an incomplete task");
    }

    internal void SetResult()
    {
        if (_core == null)
            throw new InvalidOperationException("Tried to set result of a completed task");

        _core.SetResult(Unit.V);
    }

    public bool Equals(AlbionTask other) => Equals(_core, other._core);
    public override bool Equals(object? obj) => obj is AlbionTask other && Equals(other);
    public static bool operator ==(AlbionTask x, AlbionTask y) => x.Equals(y);
    public static bool operator !=(AlbionTask x, AlbionTask y) => !(x == y);
    public override int GetHashCode() => _core?.GetHashCode() ?? 0;
}
