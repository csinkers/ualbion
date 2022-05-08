using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public interface ICoreFactory
{
    ISkybox CreateSkybox(ITexture texture);
    SpriteBatch<TInstance> CreateSpriteBatch<TInstance>(SpriteKey key) where TInstance : unmanaged;
}