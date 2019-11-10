using System.Collections.Generic;
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

        readonly IList<(string, float)> _activeTimers = new List<(string, float)>();
        float _elapsedTimeThisGameFrame;
        float _totalElapsedTime;
        bool _running = false;

        public GameClock() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<GameClock, StartClockEvent>((x,e) => x._running = true),
            H<GameClock, StopClockEvent>((x,e) => x._running = false),
            H<GameClock, EngineUpdateEvent>((x,e) => x.OnEngineUpdate(e)),
            H<GameClock, StartTimerEvent>((x,e) => x.StartTimer(e))
        );

        void StartTimer(StartTimerEvent e) => _activeTimers.Add((e.Id, _totalElapsedTime + e.IntervalMilliseconds/1000));

        void OnEngineUpdate(EngineUpdateEvent e)
        {
            _totalElapsedTime += e.DeltaSeconds;
            for (int i = 0; i < _activeTimers.Count; i++)
            {
                if (!(_activeTimers[i].Item2 <= _totalElapsedTime)) 
                    continue;

                Raise(new TimerElapsedEvent(_activeTimers[i].Item1));
                _activeTimers.RemoveAt(i);
                i--;
            }

            if (_running)
            {
                _elapsedTimeThisGameFrame += e.DeltaSeconds;
                if (_elapsedTimeThisGameFrame > TickDurationSeconds)
                {
                    _elapsedTimeThisGameFrame -= TickDurationSeconds;
                    Raise(new UpdateEvent(1));

                    var stateManager = Resolve<IStateManager>();
                    if ((stateManager?.FrameCount ?? 0) % TicksPerCacheCycle == TicksPerCacheCycle - 1) Raise(new CycleCacheEvent());
                }

                // If the game was paused for a while don't try and catch up
                if (_elapsedTimeThisGameFrame > 2 * TickDurationSeconds) _elapsedTimeThisGameFrame = 0;
            }

            Raise(new PostUpdateEvent());
        }
    }
}