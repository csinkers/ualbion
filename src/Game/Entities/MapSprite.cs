using System;
using System.Numerics;
using Superpower.Model;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Entities;

public class MapSprite : Sprite
{
    public Vector3 TilePosition
    {
        get => Position / Resolve<IMapManager>().Current.TileSize;
        set
        {
            var map = Resolve<IMapManager>().Current;
            var tilePosition = Position / map.TileSize;
            if (tilePosition == value)
                return;

            Position = value * map.TileSize;
        }
    }

    public override string ToString() => $"MapSprite {Id} @ {TilePosition}";
    public MapSprite(SpriteId id, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags)
        : base(id, Vector3.Zero, layer, keyFlags, flags) { }
}