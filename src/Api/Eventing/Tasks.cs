#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace UAlbion.Api.Eventing;

public static class Tasks
{
    static int _nextId;
    static readonly object SyncRoot = new();
    static readonly List<IAlbionTaskCore> Pending = new(); // Just for debugging

    public static int GetNextId() => Interlocked.Increment(ref _nextId);
    public static void AddTask(IAlbionTaskCore task)
    {
        lock(SyncRoot)
            Pending.Add(task);
    }

    public static void RemoveTask(IAlbionTaskCore task)
    {
        lock (SyncRoot)
            Pending.Remove(task);
    }

    public static void EnumeratePendingTasks<TContext>(TContext context, Action<TContext, IAlbionTaskCore> func)
    {
        lock (SyncRoot)
        {
            foreach (var task in Pending)
                func(context, task);
        }
    }
}