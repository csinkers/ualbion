using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("prompt:combat_positions")]
public class ShowCombatPositionsDialogEvent : Event, IVerboseEvent { }