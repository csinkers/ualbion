using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Veldrid.Visual;

public class SimpleMapLayerInfoBuilder : IMapLayerInfoBuilder<SpriteInfo>
{
    static readonly Region BlankRegion = new(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0);
    readonly ITileGraphics _tileset;
    public Vector2 TileSize { get; }

    public SimpleMapLayerInfoBuilder(ITileGraphics tileset, Vector2 tileSize)
    {
        _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));
        TileSize = tileSize;
    }

    public SpriteInfo BlankInstance { get; } = new(0, Vector3.Zero, Vector2.Zero, BlankRegion);
    public SpriteKey GetSpriteKey(DrawLayer drawLayer, SpriteKeyFlags flags) => new(_tileset.Texture, SpriteSampler.Point, drawLayer, flags);
    public SpriteInfo BuildInstance(Vector3 position, ushort imageNumber, SpriteFlags flags)
    {
        var subImage = _tileset.GetRegion(imageNumber, 0);
        return new SpriteInfo(flags, position, TileSize, subImage);
    }

    public bool IsAnimated(TileData tile)
    {
        if (tile == null)
            return false;

        return tile.FrameCount > 1 || _tileset.IsPaletteAnimated(tile.ImageNumber);
    }
}