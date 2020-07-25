using UAlbion.Api;

namespace UAlbion.Game.Events
{
    public interface IGameEvent : IEvent { }
    public abstract class GameEvent : Event, IGameEvent { }
}
