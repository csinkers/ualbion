using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Tests;

public class MockGameFactory : Component, IGameFactory
{
    readonly Action<Span<SpriteInfo>> _disableSprites = DisableSprites;
    readonly Action<Span<BlendedSpriteInfo>> _disableBlendedSprites = DisableBlendedSprites;

    protected override void Subscribed()
    {
        Exchange.Register<IGameFactory>(this);
        Exchange.Register<ICoreFactory>(this);
        base.Subscribed();
    }

    protected override void Unsubscribed()
    {
        base.Unsubscribed();
        Exchange.Unregister(this);
    }

    public ISkybox CreateSkybox(ITexture texture, ICamera camera)
    {
        throw new NotImplementedException();
    }

    public RenderableBatch<SpriteKey, SpriteInfo> CreateSpriteBatch(SpriteKey key) => new MockSpriteBatch<SpriteInfo>(key, _disableSprites);
    public RenderableBatch<SpriteKey, BlendedSpriteInfo> CreateBlendedSpriteBatch(SpriteKey key) => new MockSpriteBatch<BlendedSpriteInfo>(key, _disableBlendedSprites);

    public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITileGraphics tileset, Vector2 tileSize)
        => throw new NotImplementedException();

    static void DisableSprites(Span<SpriteInfo> instances)
    {
        for (int i = 0; i < instances.Length; i++)
        {
            ref var instance = ref instances[i];
            instance.Flags = 0;
            instance.Position = new Vector4(1e12f, 1e12f, 1e12f, 0);
            instance.Size = Vector2.Zero;
        }
    }

    static void DisableBlendedSprites(Span<BlendedSpriteInfo> instances)
    {
        for (int i = 0; i < instances.Length; i++)
        {
            ref var instance = ref instances[i];
            instance.Flags = 0;
            instance.Position = new Vector4(1e12f, 1e12f, 1e12f, 0);
            instance.Size = Vector2.Zero;
        }
    }
}