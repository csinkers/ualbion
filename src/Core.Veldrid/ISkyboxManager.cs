using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Skybox;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public interface ISkyboxManager : IRenderableSource
{
    SkyboxRenderable CreateSkybox(ITexture texture, ICamera camera);
}