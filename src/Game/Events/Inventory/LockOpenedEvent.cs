using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:lock_opened")] public class LockOpenedEvent : Event { }