using UAlbion.Core;

namespace UAlbion.Game.Events
{
    public class StartTimerEvent : GameEvent
    {
        public StartTimerEvent(string id, float intervalMilliseconds, IComponent target)
        {
            Id = id;
            IntervalMilliseconds = intervalMilliseconds;
            Target = target;
        }

        public string Id { get; }
        public float IntervalMilliseconds { get; }
        public IComponent Target { get; }
    }
}