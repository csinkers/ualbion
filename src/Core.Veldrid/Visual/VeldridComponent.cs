using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Visual
{
    public abstract class VeldridComponent : Component, IVisualComponent
    {
        public abstract void CreateDeviceObjects(VeldridRendererContext context);
        public abstract void DestroyDeviceObjects();
        public void CreateDeviceObjects(IRendererContext context) => CreateDeviceObjects((VeldridRendererContext)context);
    }
}