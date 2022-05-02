using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events.Inventory;

[Event("take_all", "Take everything from the currently open chest")]
public class TakeAllEvent : GameEvent { }