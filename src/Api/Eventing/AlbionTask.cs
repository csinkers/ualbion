#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UAlbion.Api.Eventing;

[AsyncMethodBuilder(typeof(AlbionTaskBuilder))]
public readonly struct AlbionTask : INotifyCompletion, IEquatable<AlbionTask>
{
    public static AlbionTask Complete { get; } = new(null);
    public static AlbionTask<Unit> Unit { get; } = new(Api.Unit.V);
    public static AlbionTask<bool> True { get; } = new(true);
    public static AlbionTask<bool> False { get; } = new(false);
    public static AlbionTask<T> FromResult<T>(T result) => new(result);

    readonly IAlbionTaskCore? _core;

    internal AlbionTask(IAlbionTaskCore? core) => _core = core;
    public AlbionTask GetAwaiter() => this;
    public bool IsCompleted => _core == null || _core.IsCompleted;

    public AlbionTask Named(string description)
    {
#if DEBUG
        if (_core != null)
            _core.Description = description;
#endif

        return this;
    }

    public void OnCompleted(Action continuation)
    {
        if (_core == null) // If it's already completed then call the continuation immediately.
        {
            continuation();
            return;
        }


        _core.OnCompleted(continuation);
    }

    public void GetResult()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Tried to get result of an incomplete task");
    }

    public bool Equals(AlbionTask other) => Equals(_core, other._core);
    public override bool Equals(object? obj) => obj is AlbionTask other && Equals(other);
    public static bool operator ==(AlbionTask x, AlbionTask y) => x.Equals(y);
    public static bool operator !=(AlbionTask x, AlbionTask y) => !(x == y);
    public override int GetHashCode() => _core?.GetHashCode() ?? 0;
    public override string ToString() => _core == null ? "AT[]" : "AT(Pending)";
}

/// <summary>
/// A much simpler alternative to Task. Handles all async on the main thread.
/// </summary>
[AsyncMethodBuilder(typeof(AlbionTaskBuilder<>))]
public readonly struct AlbionTask<T> : INotifyCompletion, IEquatable<AlbionTask<T>>
{
    readonly AlbionTaskCore<T>? _core;
    readonly T? _result;

    /// <summary>
    /// Create a completed task
    /// </summary>
    public AlbionTask(T value)
    {
        _core = null;
        _result = value;
    }

    /// <summary>
    /// Create a task that will complete later
    /// </summary>
    internal AlbionTask(AlbionTaskCore<T> core)
    {
        _core = core;
        _result = default;
    }

    public AlbionTask AsUntyped => new(_core);
    public AlbionTask<T> GetAwaiter() => this;

    public AlbionTask<T> Named(string description)
    {
#if DEBUG
        if (_core != null)
            _core.Description = description;
#endif

        return this;
    }

    public bool IsCompleted => _core == null || _core.IsCompleted;

    public void OnCompleted(Action continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        if (_core == null)
        {
            continuation();
            return;
        }

        _core.OnCompleted(continuation);
    }

    public T GetResult() => _core == null ? _result! : _core.GetResult();

    public bool Equals(AlbionTask<T> other) => Equals(_core, other._core) && EqualityComparer<T?>.Default.Equals(_result, other._result);
    public override bool Equals(object? obj) => obj is AlbionTask<T> other && Equals(other);
    public static bool operator ==(AlbionTask<T> x, AlbionTask<T> y) => x.Equals(y);
    public static bool operator !=(AlbionTask<T> x, AlbionTask<T> y) => !(x == y);
    public override int GetHashCode() => HashCode.Combine(_core, _result);
    public override string ToString() => _core == null ? $"AT[{_result}]" : "AT(Pending)";
}
