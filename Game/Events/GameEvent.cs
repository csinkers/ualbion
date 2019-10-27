using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public interface IGameEvent : IEvent { }
    public interface INpcEvent : IGameEvent { NpcCharacterId NpcId { get; } }
    public abstract class GameEvent : Event, IGameEvent { }
}
