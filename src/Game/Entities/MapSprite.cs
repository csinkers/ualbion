using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Entities;

public class MapSprite : Sprite
{
    readonly Vector3 _tileSize;

    [DiagEdit(Style = DiagEditStyle.TilePosition3D)]
    public Vector3 TilePosition
    {
        get => Position / _tileSize;
        set
        {
            var tilePosition = Position / _tileSize;
            if (tilePosition == value)
                return;

            Position = value * _tileSize;
        }
    }

    public override string ToString() => $"MapSprite {Id} @ {TilePosition}";

    public MapSprite(
        SpriteId id,
        Vector3 tileSize,
        DrawLayer layer,
        SpriteKeyFlags keyFlags,
        SpriteFlags flags,
        Func<IAssetId, ITexture> textureLoaderFunc = null,
        IBatchManager<SpriteKey, SpriteInfo> batchManager = null)
        : base(id, layer, keyFlags, flags, textureLoaderFunc, batchManager)
    {
        _tileSize = tileSize;
    }
}