using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class RenderEvent : EngineEvent, IVerboseEvent
    {
        public static RenderEvent Instance { get; } = new();
    }
}
