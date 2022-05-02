using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("set_combat_delay")]
public class SetCombatDelayEvent : GameEvent
{
    public SetCombatDelayEvent(int value)
    {
        Value = value;
    }

    [EventPart("value")]
    public int Value { get; }
}