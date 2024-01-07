#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UAlbion.Api.Eventing;

public interface IAlbionTaskCore : ICriticalNotifyCompletion
{
    bool IsCompleted { get; }
}

internal class AlbionTaskCore<T> : IAlbionTaskCore
{
    object? _continuation;
    T? _result;

    public bool IsCompleted { get; private set; }
    public AlbionTask<T> Task => new(this);
    internal int OutstandingCompletions { get; set; }

    public void OnCompleted(Action continuation)
    {
        if (continuation == null) throw new ArgumentNullException(nameof(continuation));

        if (IsCompleted)
        {
            continuation();
            return;
        }

        if (_continuation == null)
        {
            _continuation = continuation;
            return;
        }

        if (_continuation is not List<Action> list)
        {
            list = new List<Action>();
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

    internal void SetResult(T value)
    {
        _result = value;
        IsCompleted = true;

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
    }
}