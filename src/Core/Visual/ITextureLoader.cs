using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public interface ITextureLoader
    {
        ITexture LoadTexture(IAssetId id);
    }
}
