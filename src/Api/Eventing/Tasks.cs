#nullable enable
using System.Collections.Generic;
using System.Threading;

namespace UAlbion.Api.Eventing;

internal static class Tasks
{
    static int _nextId;
    static readonly object SyncRoot = new();
    public static List<IAlbionTaskCore> Pending { get; } = new(); // Just for debugging

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
}