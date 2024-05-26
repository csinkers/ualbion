#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UAlbion.Api.Eventing;

public interface IAlbionTaskCore : ICriticalNotifyCompletion
{
#if DEBUG
    string? Description { get; set; }
#endif
    bool IsCompleted { get; }
}

public class AlbionTaskCore<T> : IAlbionTaskCore
{
#if DEBUG
    enum TaskStatus
    {
        Unused,
        Awaited,
        Completing,
        Completed
    }

    readonly int _id;
    TaskStatus _status = TaskStatus.Unused;
    public bool BreakOnCompletion { get; set; }
    public string? Description { get; set; }
#endif

#if RECORD_TASK_STACKS
    string _stack;
#endif

    object? _continuation;
    T? _result;

    public bool IsCompleted { get; private set; }
    public AlbionTask<T> Task => new(this);
    public AlbionTask UntypedTask => new(this);
    internal int OutstandingCompletions { get; set; }

    public AlbionTaskCore() : this(null) { }
    public AlbionTaskCore(string? description)
    {
#if DEBUG
        _id = Tasks.GetNextId();
        Description = description;
        Tasks.AddTask(this);
#endif
#if RECORD_TASK_STACKS
        _stack = Environment.StackTrace;
#endif
    }

    public override string ToString()
    {
        int waiters = _continuation switch
            {
                List<Action> list => list.Count,
                Action => 1,
                _ => 0
            };

#if DEBUG
        return Description != null 
            ? $"T{_id}<{typeof(T)}>: [{_status}] ({waiters} waiting): {Description}" 
            : $"T{_id}<{typeof(T)}>: [{_status}] ({waiters} waiting)";
#else
        return $"T<{nameof(T)}>: ({waiters} waiting)";
#endif
    }

    public void OnCompleted(Action continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        if (IsCompleted)
        {
            continuation();
            return;
        }

#if DEBUG
        _status = TaskStatus.Awaited;
#endif

        if (_continuation == null)
        {
            _continuation = continuation;
            return;
        }

        if (_continuation is not List<Action> list)
        {
            list = new List<Action> { (Action)_continuation };
            _continuation = list;
        }

        list.Add(continuation);
    }

    public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);

    public T GetResult()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Tried to get result of an incomplete task");

        return _result!;
    }

    public void SetResult(T value)
    {
        _result = value;
        IsCompleted = true;
#if DEBUG
        if (BreakOnCompletion)
            Debugger.Break();

        _status = TaskStatus.Completing;
#endif

        switch (_continuation)
        {
            case Action action:
                action();
                break;

            case List<Action> list:
                {
                    foreach (var action in list)
                        action();
                    break;
                }
        }

#if DEBUG
        _status = TaskStatus.Completed;
        Tasks.RemoveTask(this);
#endif
    }
}

public class AlbionTaskCore : AlbionTaskCore<Unit>
{
    public AlbionTaskCore() : base() { }
    public AlbionTaskCore(string description) : base(description) { }
    public void Complete() => SetResult(Unit.V);
}
