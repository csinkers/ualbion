using Veldrid;

namespace UAlbion.Core
{
    public interface ITextureManager
    {
        TextureView GetTexture(ITexture texture);
        void PrepareTexture(ITexture texture, GraphicsDevice gd);
        void DestroyDeviceObjects();
    }
}