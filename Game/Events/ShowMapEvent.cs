using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("show_map")]
    public class ShowMapEvent : GameEvent
    {
        public ShowMapEvent(bool show = true) => Show = show;

        [EventPart("show", true)] public bool Show { get; }

    }
}
