using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

[Event("stop_combat_clock", "Stop the combat clock from advancing automatically.")]
public class StopCombatClockEvent : GameEvent { }