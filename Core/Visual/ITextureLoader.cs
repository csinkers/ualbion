using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public interface ITextureLoader
    {
        ITexture LoadTexture<T>(T id);
    }
}
