using System.Runtime.CompilerServices;

namespace AsyncTests;

public sealed class AlbionTaskBuilder
{
    public AlbionTask Task => new();
    public AlbionTaskBuilder() { } 
    public static AlbionTaskBuilder Create() => new();
    public void SetResult()
    {
        Console.WriteLine("SetResult");
        Task.SetResult();
    }

    public void SetException(Exception exception) => throw exception;

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        awaiter.OnCompleted(stateMachine.MoveNext);

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        awaiter.OnCompleted(stateMachine.MoveNext);

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        Console.WriteLine("Start");
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
}