using UAlbion.Api.Eventing;

namespace UAlbion.Game.Combat;

[Event("combat_update", "Run the combat clock for the specified number of slow-clock cycles")]
public class CombatUpdateEvent : Event, IAsyncEvent
{
    public CombatUpdateEvent(int cycles) => Cycles = cycles;

    [EventPart("cycles", "The number of slow-clock cycles to update for")]
    public int Cycles { get; }
}