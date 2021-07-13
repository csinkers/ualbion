using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("post_update")]
    public class PostGameUpdateEvent : GameEvent, IVerboseEvent
    {
    }
}
