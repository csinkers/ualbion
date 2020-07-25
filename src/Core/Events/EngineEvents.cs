using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public interface IEngineEvent : IEvent { }

    public abstract class EngineEvent : Event, IEngineEvent { }
}
