using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

[Event("combat_clock")]
public class CombatClockEvent : GameEvent, IVerboseEvent { }