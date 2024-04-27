using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

public class CombatClockTimerEvent : GameEvent , IVerboseEvent
{
    public CombatClockTimerEvent(int combatTicks)
    {
        CombatTicks = combatTicks;
    }

    public int CombatTicks { get; }
}