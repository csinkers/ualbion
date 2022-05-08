using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public class TrueColorTileGraphics : ITileGraphics
{
    readonly List<(int offset, int palCount)> _dayFrames;
    readonly List<(int offset, int palCount)> _nightFrames;
    readonly IReadOnlyTexture<uint> _texture;
    public ITexture Texture => _texture;
    public TrueColorTileGraphics(
        IReadOnlyTexture<uint> texture,
        List<(int offset, int palCount)> dayFrames,
        List<(int offset, int palCount)> nightFrames)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _dayFrames = dayFrames;
        _nightFrames = nightFrames;
    }

    public Region GetRegionInner(List<(int offset, int palCount)> frames, int imageNumber, int paletteFrame)
    {
        var info = frames[imageNumber];
        paletteFrame %= info.palCount;
        int subImage = info.offset + paletteFrame;
        return _texture.Regions[subImage];
    }

    public Region GetRegion(int imageNumber, int paletteFrame) => GetRegionInner(_dayFrames, imageNumber, paletteFrame); 
    public Region GetNightRegion(int imageNumber, int paletteFrame) => GetRegionInner(_nightFrames ?? _dayFrames, imageNumber, paletteFrame);
}