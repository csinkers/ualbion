using System;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class VeldridCoreFactory : ServiceComponent<ICoreFactory>, ICoreFactory
{
    public ISkybox CreateSkybox(ITexture texture)
        => Resolve<ISkyboxManager>().CreateSkybox(texture);

    public SpriteBatch<TInstance> CreateSpriteBatch<TInstance>(SpriteKey key) where TInstance : unmanaged
    {
        if (typeof(TInstance) == typeof(SpriteInfo))
            return new VeldridSpriteBatch<TInstance, GpuSpriteInstanceData>(key);

        if (typeof(TInstance) == typeof(BlendedSpriteInfo))
            return new VeldridSpriteBatch<TInstance, GpuBlendedSpriteInstanceData>(key);

        throw new NotSupportedException($"Unhandled sprite instance data type {typeof(TInstance).Name}");
    }
}
