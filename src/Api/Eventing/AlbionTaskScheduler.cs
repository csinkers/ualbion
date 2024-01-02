/*
using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Api.Eventing;

public class AlbionTaskScheduler
{
    public static AlbionTaskScheduler Default { get; } = new();
    readonly List<AlbionTask> scheduledTasks = new();

    /// <summary>
    /// Executes the scheduled Tasks synchronously on the current thread. If those tasks schedule new tasks
    /// they will also be executed until no pending tasks are left.
    /// </summary>
    public void RunTasksUntilIdle()
    {
        while (scheduledTasks.Any())
            RunPendingTasks();
    }

    /// <summary>
    /// Executes the scheduled Tasks synchronously on the current thread. If those tasks schedule new tasks
    /// they will only be executed with the next call to RunTasksUntilIdle() or RunPendingTasks(). 
    /// </summary>
    public void RunPendingTasks()
    {
        foreach (var task in scheduledTasks.ToArray())
        {
            Console.WriteLine("ExecuteTask");
            // task.???
            scheduledTasks.Remove(task);
        }
    }

    public void QueueTask(AlbionTask task) => scheduledTasks.Add(task);
    protected IEnumerable<AlbionTask> GetScheduledTasks() => scheduledTasks;
}
*/