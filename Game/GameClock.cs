using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class HourElapsedEvent : GameEvent { }
    public class DayElapsedEvent : GameEvent { }

    public class GameClock : ServiceComponent<IClock>, IClock
    {
        const float TickDurationSeconds = 1 / 60.0f;
        const float GameSecondsPerSecond = 60;
        const int TicksPerCacheCycle = 3600; // Cycle the cache every minute

        readonly IList<(string, float)> _activeTimers = new List<(string, float)>();
        float _elapsedTimeThisGameFrame;
        int _slowTicksRemaining;
        UpdateEvent _updateEvent;

        public GameClock()
        {
            On<StartClockEvent>(e => IsRunning = true);
            On<StopClockEvent>(e => IsRunning = false);
            On<EngineUpdateEvent>(OnEngineUpdate);
            On<SlowClockEvent>(OnSlowClock);
            On<StartTimerEvent>(StartTimer);
            On<UpdateEvent>(e =>
            {
                if (IsRunning || _updateEvent != null)
                    return;

                e.Acknowledged = true;
                _updateEvent = e;
                _slowTicksRemaining = e.Cycles;
                IsRunning = true;
            });
        }

        public DateTime GameTime { get; private set; } = new DateTime(2000, 1, 1);
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }

        void OnSlowClock(SlowClockEvent e)
        {
            if (_slowTicksRemaining <= 0)
                return;

            _slowTicksRemaining -= e.Delta;
            if (_slowTicksRemaining > 0)
                return;

            IsRunning = false;
            var updateEvent = _updateEvent;
            _updateEvent = null;
            updateEvent?.Complete();
        }

        void StartTimer(StartTimerEvent e) => _activeTimers.Add((e.Id, ElapsedTime + e.IntervalMilliseconds / 1000));

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

            if (IsRunning)
            {
                var lastGameTime = GameTime;
                GameTime += TimeSpan.FromSeconds(e.DeltaSeconds * GameSecondsPerSecond);
                if (GameTime.Hour != lastGameTime.Hour)
                    Raise(new HourElapsedEvent());

                if (GameTime.Date != lastGameTime.Date)
                    Raise(new DayElapsedEvent());

                var state = Resolve<IGameState>();
                _elapsedTimeThisGameFrame += e.DeltaSeconds;

                // If the game was paused for a while don't try and catch up
                if (_elapsedTimeThisGameFrame > 4 * TickDurationSeconds) _elapsedTimeThisGameFrame = 4 * TickDurationSeconds;

                while (_elapsedTimeThisGameFrame >= TickDurationSeconds)
                {
                    _elapsedTimeThisGameFrame -= TickDurationSeconds;
                    Raise(new FastClockEvent(1));

                    if ((state?.TickCount ?? 0) % TicksPerCacheCycle == TicksPerCacheCycle - 1)
                        Raise(new CycleCacheEvent());
                }
            }

            Raise(new PostUpdateEvent());
        }
    }
}
