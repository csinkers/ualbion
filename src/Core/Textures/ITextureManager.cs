using UAlbion.Core.Visual;

namespace UAlbion.Core.Textures
{
    public interface ITextureManager : IComponent
    {
        object GetTexture(ITexture texture);
        void PrepareTexture(ITexture texture, IRendererContext context);
        void DestroyDeviceObjects();
        string Stats();
    }
}
