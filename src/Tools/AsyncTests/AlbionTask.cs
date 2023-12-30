using System.Runtime.CompilerServices;

namespace AsyncTests;
/// <summary>
/// A much simpler alternative to Task. Handles all async on the main thread,
/// each frame any queued unblocked AlbionTasks will be progressed.
/// </summary>
[AsyncMethodBuilder(typeof(AlbionTaskBuilder))]
public class AlbionTask : INotifyCompletion
{
    public static AlbionTask CompletedTask { get; } = new() { IsCompleted = true };
    object? _continuation;
    public AlbionTask GetAwaiter() => this;
    public bool IsCompleted { get; private set; }

    public void OnCompleted(Action continuation)
    {
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

    internal void SetResult()
    {
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
    public void GetResult()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Tried to get result of an incomplete task");
    }

    public void Start() => AlbionTaskScheduler.Default.QueueTask(this);
}
public class AlbionTask<T> : INotifyCompletion
{
    object? _continuation;
    T? _result;
    public static AlbionTask<T> FromResult(T value) =>
        new()
        {
            _result = value,
            IsCompleted = true
        };

    public AlbionTask<T> GetAwaiter() => this;
    public bool IsCompleted { get; private set; }
    public void OnCompleted(Action continuation)
    {
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