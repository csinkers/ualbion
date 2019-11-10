namespace UAlbion.Game.Events
{
    public class TimerElapsedEvent : GameEvent
    {
        public TimerElapsedEvent(string id) { Id = id; }
        public string Id { get; }
    }
}