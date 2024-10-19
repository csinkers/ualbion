using UAlbion.Api.Eventing;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class PrepareDeviceObjectsEvent : Event, IVerboseEvent
{
    public GraphicsDevice Device { get; set; }
}