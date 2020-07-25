using UAlbion.Core;

namespace UAlbion.Game.Events
{
    public class StartTimerEvent : GameEvent
    {
        public StartTimerEvent(string id, float intervalSeconds, IComponent target)
        {
            Id = id;
            IntervalSeconds = intervalSeconds;
            Target = target;
        }

        public string Id { get; }
        public float IntervalSeconds { get; }
        public IComponent Target { get; }
    }
}
