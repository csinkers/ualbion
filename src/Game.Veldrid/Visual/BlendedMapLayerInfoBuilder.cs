using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Veldrid.Visual;

public class BlendedMapLayerInfoBuilder : Component, IMapLayerInfoBuilder<BlendedSpriteInfo>
{
    readonly TrueColorTileGraphics _tileset;
    IPaletteManager _paletteManager;

    public Vector2 TileSize { get; }
    static readonly Region BlankRegion = new(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0);
    public BlendedSpriteInfo BlankInstance { get; } = new(0, Vector3.Zero, Vector2.Zero, BlankRegion, BlankRegion);
    public SpriteKey GetSpriteKey(DrawLayer drawLayer, SpriteKeyFlags spriteKeyFlags) => new(_tileset.Texture, SpriteSampler.TriLinear, drawLayer, spriteKeyFlags);
    protected override void Subscribed() => _paletteManager = Resolve<IPaletteManager>();
    public BlendedMapLayerInfoBuilder(TrueColorTileGraphics tileset, Vector2 tileSize)
    {
        _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));
        TileSize = tileSize;
    }

    public BlendedSpriteInfo BuildInstance(Vector3 position, ushort imageNumber, SpriteFlags flags)
    {
        var palFrame = _paletteManager.Frame;
        var day = _tileset.GetRegion(imageNumber, palFrame);
        var night = _tileset.GetNightRegion(imageNumber, palFrame);
        return new BlendedSpriteInfo(flags, position, TileSize, day, night);
    }

    public bool IsAnimated(TileData tile)
    {
        if (tile == null)
            return false;

        return tile.FrameCount > 1 || _tileset.IsPaletteAnimated(tile.ImageNumber);
    }
}