using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid
{
    public interface ISkyboxManager : IRenderableSource
    {
        Skybox.SkyboxRenderable CreateSkybox(ITexture texture);
    }
}