using System;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class VeldridCoreFactory : ServiceComponent<ICoreFactory>, ICoreFactory
{
    readonly Func<MeshId, Mesh> _meshLoaderFunc;

    public VeldridCoreFactory(Func<MeshId, Mesh> meshLoaderFunc) 
        => _meshLoaderFunc = meshLoaderFunc ?? throw new ArgumentNullException(nameof(meshLoaderFunc));

    public ISkybox CreateSkybox(ITexture texture, ICamera camera)
        => Resolve<ISkyboxManager>().CreateSkybox(texture, camera);

    public RenderableBatch<SpriteKey, SpriteInfo> CreateSpriteBatch(SpriteKey key) => new VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData>(key);
    public RenderableBatch<SpriteKey, BlendedSpriteInfo> CreateBlendedSpriteBatch(SpriteKey key) => new VeldridSpriteBatch<BlendedSpriteInfo, GpuBlendedSpriteInstanceData>(key);
#pragma warning disable CA1822
    public RenderableBatch<MeshId, GpuMeshInstanceData> CreateMeshBatch(MeshId id) => new MeshBatch(id, _meshLoaderFunc);
#pragma warning restore CA1822
}