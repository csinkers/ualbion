using System;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game.Events;

namespace UAlbion.Game;

public abstract class GameComponent : Component
{
    protected IAssetManager Assets => Resolve<IAssetManager>();

    protected async AlbionTask WithFrozenClock<T>(T context, Func<T, AlbionTask> func)
    {
        var wasClockRunning = Resolve<IClock>()?.IsRunning ?? false;
        if (wasClockRunning)
            Raise(StopClockEvent.Instance);

        await func(context);

        if (wasClockRunning)
            Raise(StartClockEvent.Instance);
    }
}

public abstract class GameServiceComponent<T> : ServiceComponent<T>
{
    protected IAssetManager Assets => Resolve<IAssetManager>();

    protected async AlbionTask WithFrozenClock<TContext>(TContext context, Func<TContext, AlbionTask> func)
    {
        var wasClockRunning = Resolve<IClock>()?.IsRunning ?? false;
        if (wasClockRunning)
            Raise(StopClockEvent.Instance);

        await func(context);

        if (wasClockRunning)
            Raise(StartClockEvent.Instance);
    }
}