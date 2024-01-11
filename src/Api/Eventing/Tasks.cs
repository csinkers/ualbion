#nullable enable
using System.Collections.Generic;
using System.Threading;

namespace UAlbion.Api.Eventing;

internal static class Tasks
{
    static int _nextId;
    static readonly object _syncRoot = new();
    public static List<IAlbionTaskCore> Pending { get; } = new(); // Just for debugging

    public static int GetNextId() => Interlocked.Increment(ref _nextId);
    public static void AddTask(IAlbionTaskCore task)
    {
        lock(_syncRoot)
            Pending.Add(task);
    }

    public static void RemoveTask(IAlbionTaskCore task)
    {
        lock (_syncRoot)
            Pending.Remove(task);
    }
}