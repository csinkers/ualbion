using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public class TrueColorTileGraphics : ITileGraphics
{
    readonly List<(int offset, int frameCount, int palCount)> _frameInfo;
    readonly IReadOnlyTexture<uint> _texture;
    public ITexture Texture => _texture;
    public TrueColorTileGraphics(IReadOnlyTexture<uint> texture, List<(int offset, int frameCount, int palCount)> frameInfo)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _frameInfo = frameInfo;
    }

    public Region GetRegion(int imageNumber, int frame, int paletteFrame)
    {
        var info = _frameInfo[imageNumber];
        int subImage = info.offset + frame * info.palCount + paletteFrame;
        return _texture.Regions[subImage];
    }
}