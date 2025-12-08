using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class VeldridCoreFactory : ServiceComponent<ICoreFactory>, ICoreFactory
{
    readonly Func<MeshId, Mesh> _meshLoaderFunc;
    readonly Action<Span<SpriteInfo>> _disableSpriteInfos;
    readonly Action<Span<BlendedSpriteInfo>> _disableBlendedSpriteInfos;

    public VeldridCoreFactory(Func<MeshId, Mesh> meshLoaderFunc)
    {
        _meshLoaderFunc = meshLoaderFunc ?? throw new ArgumentNullException(nameof(meshLoaderFunc));
        _disableSpriteInfos = DisableSpriteInfos;
        _disableBlendedSpriteInfos = DisableBlendedSpriteInfos;
    }

    public ISkybox CreateSkybox(ITexture texture, ICamera camera)
        => Resolve<ISkyboxManager>().CreateSkybox(texture, camera);

    public RenderableBatch<SpriteKey, SpriteInfo> CreateSpriteBatch(SpriteKey key) => new VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData>(key, _disableSpriteInfos);
    public RenderableBatch<SpriteKey, BlendedSpriteInfo> CreateBlendedSpriteBatch(SpriteKey key) => new VeldridSpriteBatch<BlendedSpriteInfo, GpuBlendedSpriteInstanceData>(key, _disableBlendedSpriteInfos);

    static void DisableSpriteInfos(Span<SpriteInfo> instances)
    {
        for (var index = 0; index < instances.Length; index++)
        {
            ref var instance = ref instances[index];
            instance.Flags = 0;
            instance.Size = Vector2.Zero;
            instance.Position = new Vector4(1e12f, 1e12f, 1e12f, 0);
        }
    }

    static void DisableBlendedSpriteInfos(Span<BlendedSpriteInfo> instances)
    {
        for (var index = 0; index < instances.Length; index++)
        {
            ref var instance = ref instances[index];
            instance.Flags = 0;
            instance.Size = Vector2.Zero;
            instance.Position = new Vector4(1e12f, 1e12f, 1e12f, 0);
        }
    }

#pragma warning disable CA1822
    public RenderableBatch<MeshId, GpuMeshInstanceData> CreateMeshBatch(MeshId id) => new MeshBatch(id, _meshLoaderFunc);
#pragma warning restore CA1822
}