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

    public ISkybox CreateSkybox(ITexture texture)
    {
        throw new NotImplementedException();
    }

    public SpriteBatch<TInstance> CreateSpriteBatch<TInstance>(SpriteKey key) where TInstance : unmanaged
        => new MockSpriteBatch<TInstance>(key);
    public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITileGraphics tileset, Vector2 tileSize, bool isOverlay)
        => throw new NotImplementedException();
}