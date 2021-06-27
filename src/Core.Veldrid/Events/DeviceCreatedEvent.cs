using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events
{
    public class DeviceCreatedEvent : Event
    {
        public DeviceCreatedEvent(GraphicsDevice device)
        {
            Device = device;
        }

        public GraphicsDevice Device { get; }
    }
}