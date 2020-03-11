using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("fill_screen")]
    public class FillScreenEvent : GameEvent
    {
        public FillScreenEvent(int color) { Color = color; }
        [EventPart("color")] public int Color { get; }
    }
}
