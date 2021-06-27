using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public interface ICoreFactory
    {
        ISkybox CreateSkybox(IAssetId assetId);
        ISpriteLease CreateSprites(SpriteKey key, int length, object caller);
    }
}