using Veldrid;

namespace UAlbion.Core.Textures
{
    public interface ITextureManager : IComponent
    {
        TextureView GetTexture(ITexture texture);
        void PrepareTexture(ITexture texture, GraphicsDevice gd);
        void DestroyDeviceObjects();
        string Stats();
    }
}