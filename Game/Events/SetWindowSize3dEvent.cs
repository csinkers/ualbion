using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("set_window_size")]
    public class SetWindowSize3dEvent : GameEvent
    {
        public SetWindowSize3dEvent(int value)
        {
            Value = value;
        }

        [EventPart("value")]
        public int Value { get; }
    }
}