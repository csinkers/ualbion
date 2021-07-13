using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public interface ICoreFactory
    {
        ISkybox CreateSkybox(ITexture texture);
        SpriteBatch CreateSpriteBatch(SpriteKey key);
    }
}