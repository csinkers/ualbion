using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public interface ICoreFactory
{
    ISkybox CreateSkybox(ITexture texture, ICamera camera);
    RenderableBatch<SpriteKey, SpriteInfo> CreateSpriteBatch(SpriteKey key);
    RenderableBatch<SpriteKey, BlendedSpriteInfo> CreateBlendedSpriteBatch(SpriteKey key);
}