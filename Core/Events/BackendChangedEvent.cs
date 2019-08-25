using Veldrid;

namespace UAlbion.Core.Events
{
    public class BackendChangedEvent : EngineEvent
    {
        public GraphicsDevice GraphicsDevice { get; }

        public BackendChangedEvent(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}