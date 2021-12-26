using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("ui_scroll")]
    public class UiScrollEvent : CancellableEvent, IVerboseEvent
    {
        public UiScrollEvent(int delta) => Delta = delta;
        [EventPart("delta")] public int Delta { get; set; }
    }
}