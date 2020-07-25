using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("show_map")]
    public class ShowMapEvent : GameEvent
    {
        public ShowMapEvent(bool? show = null) => Show = show;
        [EventPart("show", true)] public bool? Show { get; }
    }
}
