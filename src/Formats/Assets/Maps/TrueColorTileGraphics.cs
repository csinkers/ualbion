using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public class TrueColorTileGraphics : ITileGraphics
{
    readonly List<TileFrameSummary> _dayFrames;
    readonly List<TileFrameSummary> _nightFrames;
    readonly IReadOnlyTexture<uint> _texture;
    public ITexture Texture => _texture;
    public TrueColorTileGraphics(
        IReadOnlyTexture<uint> texture,
        List<TileFrameSummary> dayFrames,
        List<TileFrameSummary> nightFrames)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _dayFrames = dayFrames;
        _nightFrames = nightFrames;
    }

    public Region GetRegionInner(List<TileFrameSummary> frames, int imageNumber, int paletteFrame)
    {
        var info = frames[imageNumber];
        paletteFrame %= info.Paths.Length;
        return _texture.Regions[info.RegionOffset + paletteFrame];
    }

    public Region GetRegion(int imageNumber, int paletteFrame) => GetRegionInner(_dayFrames, imageNumber, paletteFrame);
    public bool IsPaletteAnimated(int imageNumber) => _dayFrames[imageNumber].Paths.Length > 1;
    public Region GetNightRegion(int imageNumber, int paletteFrame) => GetRegionInner(_nightFrames ?? _dayFrames, imageNumber, paletteFrame);
}