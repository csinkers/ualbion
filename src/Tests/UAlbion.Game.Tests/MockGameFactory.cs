﻿using System;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Tests
{
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

        public ISkybox CreateSkybox(IAssetId assetId)
        {
            throw new NotImplementedException();
        }

        public ISpriteLease CreateSprites(SpriteKey key, int length, object caller) => new MockSpriteLease(key, length);

        public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITexture tileset, Func<int, int, TileData> getTileFunc, DrawLayer layer,
            IconChangeType iconChangeType)
        {
            throw new NotImplementedException();
        }
    }
}