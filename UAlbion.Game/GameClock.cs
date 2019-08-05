using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.Game
{
    public class GameClock : Component
    {
        const float TickDurationSeconds = 1 / 8.0f;
        float _elapsed;
        bool _running = true;

        public GameClock() : base(Handlers) { }

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<GameClock, EngineUpdateEvent>((x, e) =>
            {
                if (!x._running)
                    return;

                x._elapsed += e.DeltaSeconds;
                if (x._elapsed > TickDurationSeconds)
                {
                    x._elapsed -= TickDurationSeconds;
                    x.Exchange.Raise(new UpdateEvent(1), x);
                }

                // If the game was paused for a while don't try and catch up
                if (x._elapsed > 2 * TickDurationSeconds)
                    x._elapsed = 0;
            }),
            new Handler<GameClock, StopClockEvent>((x, e) => x._running = false),
            new Handler<GameClock, StartClockEvent>((x, e) => x._running = true),
        };
    }
}