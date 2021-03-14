using UAlbion.Api.Visual;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public interface ITextureLoader
    {
        ITexture LoadTexture(IAssetId id);
    }
}
