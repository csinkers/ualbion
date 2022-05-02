using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("map_init", "Emitted after the map has been loaded to trigger any MapInit event chains")]
public class MapInitEvent : Event { }