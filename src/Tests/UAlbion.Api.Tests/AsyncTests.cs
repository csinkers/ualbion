using UAlbion.Api.Eventing;
using Xunit;

namespace UAlbion.Api.Tests;

public class AsyncTests
{
    [Fact]
    public void TestCompletedTask()
    {
        async AlbionTask Foo()
        {
            await AlbionTask.Complete;
        }

        Foo().GetResult();
    }

    [Fact]
    public void TestCompletedTaskWithResult()
    {
        async AlbionTask<int> Foo()
        {
            return await AlbionTask.FromResult(1);
        }

        var result = Foo().GetResult();
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestAwait()
    {
        int state = 0;

        var atc = new AlbionTaskCore();
        async AlbionTask Foo()
        {
            state = 1;
            await atc.Task;
            state = 2;
        }

        var awaiter = Foo().GetAwaiter();
        Assert.Equal(1, state);

        awaiter.OnCompleted(() => state = 3);

        atc.Complete();

        Assert.Equal(3, state);
    }

    [Fact]
    public void TestAwaitWithResult()
    {
        int state = 0;

        var atc = new AlbionTaskCore<int>();
        async AlbionTask<int> Foo()
        {
            state = 1;
            var result = await atc.Task;
            state = 2;
            return result;
        }

        var awaiter = Foo().GetAwaiter();
        Assert.Equal(1, state);

        awaiter.OnCompleted(() => state = awaiter.GetResult());

        atc.SetResult(3);

        Assert.Equal(3, state);
    }
}