﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace UAlbion.Api.Eventing;

public static class Tasks
{
    static int _nextId;
    static readonly Lock SyncRoot = new();
    static readonly List<IAlbionTaskCore> Pending = []; // Just for debugging
    static readonly ThreadLocal<IAlbionTaskCore?> CurrentTask = new();

    public static IAlbionTaskCore? Current
    {
        get => CurrentTask.Value;
        set => CurrentTask.Value = value;
    }

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