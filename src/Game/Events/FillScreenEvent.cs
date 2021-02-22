using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("fill_screen")] // USED IN SCRIPT
    public class FillScreenEvent : GameEvent
    {
        public FillScreenEvent(int color) { Color = color; }
        [EventPart("color")] public int Color { get; }
    }
}
