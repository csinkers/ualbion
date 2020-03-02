using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("map_init", "Emitted after the map has been loaded to trigger any MapInit event chains")]
    public class MapInitEvent : Event { }
}