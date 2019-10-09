using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class GameClock : Component
    {
        const float TickDurationSeconds = 1 / 6.0f;
        float _elapsed;
        bool _running = false;

        public GameClock() : base(Handlers) { }

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<GameClock, StopClockEvent>((x,e) => x._running = false),
            new Handler<GameClock, StartClockEvent>((x,e) => x._running = true),
            new Handler<GameClock, EngineUpdateEvent>((x, e) =>
            {
                if (x._running)
                {
                    x._elapsed += e.DeltaSeconds;
                    if (x._elapsed > TickDurationSeconds)
                    {
                        x._elapsed -= TickDurationSeconds;
                        x.Raise(new UpdateEvent(1));
                    }

                    // If the game was paused for a while don't try and catch up
                    if (x._elapsed > 2 * TickDurationSeconds)
                        x._elapsed = 0;
                }

                x.Raise(new PostUpdateEvent());
            }),
        };
    }
}