using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("day_elapsed")]
    public class DayElapsedEvent : GameEvent
    {
        public static DayElapsedEvent Instance { get; } = new();
    }
}