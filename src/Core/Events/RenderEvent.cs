using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Events;

public class RenderEvent : EngineEvent, IVerboseEvent
{
    public ICamera Camera { get; set; }
}