using UAlbion.Api;

namespace UAlbion.Game.Events
{
    public interface IGameEvent : IEvent { }
    public interface INpcEvent : IGameEvent { int NpcId { get; } }
    public abstract class GameEvent : Event, IGameEvent { }
}
