using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class VeldridCoreFactory : ServiceComponent<ICoreFactory>, ICoreFactory
{
    public ISkybox CreateSkybox(ITexture texture)
        => Resolve<ISkyboxManager>().CreateSkybox(texture);

    public RenderableBatch<SpriteKey, SpriteInfo> CreateSpriteBatch(SpriteKey key) => new VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData>(key);
    public RenderableBatch<SpriteKey, BlendedSpriteInfo> CreateBlendedSpriteBatch(SpriteKey key) => new VeldridSpriteBatch<BlendedSpriteInfo, GpuBlendedSpriteInstanceData>(key);
#pragma warning disable CA1822
    public RenderableBatch<MeshId, GpuMeshInstanceData> CreateMeshBatch(MeshId id) => new MeshBatch(id);
#pragma warning restore CA1822
}