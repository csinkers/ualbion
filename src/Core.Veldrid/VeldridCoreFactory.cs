using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class VeldridCoreFactory : ServiceComponent<ICoreFactory>, ICoreFactory
{
    public ISkybox CreateSkybox(ITexture texture) 
        => Resolve<ISkyboxManager>().CreateSkybox(texture);
    public SpriteBatch CreateSpriteBatch(SpriteKey key) 
        => new VeldridSpriteBatch(key);
}