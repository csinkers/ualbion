using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class GameClock : Component, IClock
    {
        const float TickDurationSeconds = 1 / 60.0f;
        const int TicksPerCacheCycle = 360; // Cycle the cache every minute

        readonly IList<(string, float)> _activeTimers = new List<(string, float)>();
        float _elapsedTimeThisGameFrame;
        bool _running = false;

        public GameClock() : base(Handlers) { }
        public float ElapsedTime { get; private set; }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<GameClock, StartClockEvent>((x,e) => x._running = true),
            H<GameClock, StopClockEvent>((x,e) => x._running = false),
            H<GameClock, EngineUpdateEvent>((x,e) => x.OnEngineUpdate(e)),
            H<GameClock, StartTimerEvent>((x,e) => x.StartTimer(e))
        );

        void StartTimer(StartTimerEvent e) => _activeTimers.Add((e.Id, ElapsedTime + e.IntervalMilliseconds/1000));

        void OnEngineUpdate(EngineUpdateEvent e)
        {
            ElapsedTime += e.DeltaSeconds;
            for (int i = 0; i < _activeTimers.Count; i++)
            {
                if (!(_activeTimers[i].Item2 <= ElapsedTime)) 
                    continue;

                Raise(new TimerElapsedEvent(_activeTimers[i].Item1));
                _activeTimers.RemoveAt(i);
                i--;
            }

            if (_running)
            {
                var state = Resolve<IGameState>();
                _elapsedTimeThisGameFrame += e.DeltaSeconds;

                while (_elapsedTimeThisGameFrame > TickDurationSeconds)
                {
                    _elapsedTimeThisGameFrame -= TickDurationSeconds;
                    Raise(new UpdateEvent(1));

                    if ((state?.TickCount ?? 0) % TicksPerCacheCycle == TicksPerCacheCycle - 1)
                        Raise(new CycleCacheEvent());
                }

                // If the game was paused for a while don't try and catch up
                if (_elapsedTimeThisGameFrame > 2 * TickDurationSeconds) _elapsedTimeThisGameFrame = 0;
            }

            Raise(new PostUpdateEvent());
        }
    }
}