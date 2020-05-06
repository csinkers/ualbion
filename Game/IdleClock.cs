using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class IdleClockEvent : GameEvent, IVerboseEvent { }
    public class IdleClock : Component
    {
        const float TickDurationSeconds = 1 / 8.0f;
        readonly IdleClockEvent _event = new IdleClockEvent();
        float _elapsedTimeThisGameFrame;

        public IdleClock()
        {
            On<EngineUpdateEvent>(OnEngineUpdate);
        }
        void OnEngineUpdate(EngineUpdateEvent e)
        {
            _elapsedTimeThisGameFrame += e.DeltaSeconds;

            // If the game was paused for a while don't try and catch up
            if (_elapsedTimeThisGameFrame > 4 * TickDurationSeconds)
                _elapsedTimeThisGameFrame = 4 * TickDurationSeconds;

            while (_elapsedTimeThisGameFrame >= TickDurationSeconds)
            {
                _elapsedTimeThisGameFrame -= TickDurationSeconds;
                Raise(_event);
            }
        }
    }
}