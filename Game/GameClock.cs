using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class GameClock : Component
    {
        const float TickDurationSeconds = 1 / 6.0f;
        const int TicksPerCacheCycle = 360; // Cycle the cache every minute
        float _elapsed;
        bool _running = false;

        public GameClock() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<GameClock, StartClockEvent>((x,e) => x._running = true),
            H<GameClock, StopClockEvent>((x,e) => x._running = false),
            H<GameClock, EngineUpdateEvent>((x, e) =>
            {
                if (x._running)
                {
                    x._elapsed += e.DeltaSeconds;
                    if (x._elapsed > TickDurationSeconds)
                    {
                        x._elapsed -= TickDurationSeconds;
                        x.Raise(new UpdateEvent(1));

                        var stateManager = x.Resolve<IStateManager>();
                        if((stateManager?.FrameCount ?? 0) % TicksPerCacheCycle == TicksPerCacheCycle - 1)
                            x.Raise(new CycleCacheEvent());
                    }

                    // If the game was paused for a while don't try and catch up
                    if (x._elapsed > 2 * TickDurationSeconds)
                        x._elapsed = 0;
                }

                x.Raise(new PostUpdateEvent());
            })
        );
    }
}