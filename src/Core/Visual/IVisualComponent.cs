namespace UAlbion.Core.Visual
{
    public interface IVisualComponent : IComponent
    {
        void CreateDeviceObjects(IRendererContext context);
        void DestroyDeviceObjects();
    }
}