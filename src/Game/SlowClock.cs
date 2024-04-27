using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game;

public class SlowClock : Component
{
    readonly List<(AlbionTaskCore Task, int ExpiryTickCount)> _activeTimers = new();
    readonly SlowClockEvent _event = new();
    int _ticks;
    int _slowTicks;

    public SlowClock()
    {
        On<FastClockEvent>(OnUpdate);
        OnAsync<SlowClockTimerEvent>(e =>
        {
            var atc = new AlbionTaskCore();
            _activeTimers.Add((atc, _slowTicks + e.SlowTicks));
            return atc.UntypedTask;
        });
    }

    void OnUpdate(FastClockEvent updateEvent)
    {
        _ticks += updateEvent.Frames;
        var ticksPerSlow = ReadVar(V.Game.Time.FastTicksPerSlowTick);
        while (_ticks >= ticksPerSlow)
        {
            _ticks -= ticksPerSlow;
            GameTrace.Log.SlowTick(_slowTicks++);
            Raise(_event);

            for (int i = 0; i < _activeTimers.Count; i++)
            {
                if (_slowTicks < _activeTimers[i].ExpiryTickCount)
                    continue;

                var task = _activeTimers[i].Task;
                _activeTimers.RemoveAt(i);
                i--;

                task.Complete();
            }
        }
    }
}
