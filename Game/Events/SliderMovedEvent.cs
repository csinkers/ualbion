using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("slider_moved", "Emitted when a slider is moved to a new value")]
    public class SliderMovedEvent : GameEvent
    {
        [EventPart("id", "The identifier of the slider that was moved")]
        public string SliderId { get; }

        [EventPart("value", "The value that the slider was moved to")]
        public int Position { get; }

        public SliderMovedEvent(string sliderId, int position)
        {
            SliderId = sliderId;
            Position = position;
        }
    }
}