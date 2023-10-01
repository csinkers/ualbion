using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

[Event("start_combat_clock", "Resume automatically updating the combat clock.")]
public class StartCombatClockEvent : GameEvent { }