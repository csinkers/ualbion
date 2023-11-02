namespace AsyncTests;

/// <summary>
/// TaskScheduler for executing tasks on the same thread that calls RunTasksUntilIdle() or RunPendingTasks().
/// From https://github.com/dotnet/dotnet/blob/main/src/source-build-externals/src/application-insights/BASE/Test/TestFramework/Shared/DeterministicTaskScheduler.cs
/// Copyright (c) 2015 Microsoft (MIT License https://github.com/dotnet/dotnet/blob/main/src/source-build-externals/src/application-insights/BASE/LICENSE)
/// </summary>
public class DeterministicTaskScheduler : TaskScheduler
{
    readonly List<Task> scheduledTasks = new();
    public override int MaximumConcurrencyLevel => 1;

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
            TryExecuteTask(task);
            scheduledTasks.Remove(task);
        }
    }

    protected override void QueueTask(Task task) => scheduledTasks.Add(task);
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => TryExecuteTask(task);
    protected override IEnumerable<Task> GetScheduledTasks() => scheduledTasks;
}