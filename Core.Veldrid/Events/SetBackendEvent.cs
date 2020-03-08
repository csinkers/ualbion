using UAlbion.Api;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events
{
    [Event("e:set_backend", "Sets the current graphics backend to use")]
    public class SetBackendEvent : EngineEvent
    {
        public SetBackendEvent(GraphicsBackend value) { Value = value; }
        [EventPart("value", "Valid values: OpenGL, OpenGLES, Vulkan, Metal or Direct3D11")] public GraphicsBackend Value { get; }
    }
}