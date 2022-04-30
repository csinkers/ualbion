using System;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
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

    public SpriteBatch CreateSpriteBatch(SpriteKey key)
    {
        return new MockSpriteBatch(key);
    }

    public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITexture tileset, bool isOverlay)
    {
        throw new NotImplementedException();
    }
}