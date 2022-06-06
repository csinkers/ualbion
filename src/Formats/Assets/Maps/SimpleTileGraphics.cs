using System;
using System.Text.Json.Serialization;
using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public class SimpleTileGraphics : ITileGraphics
{
    readonly IReadOnlyTexture<byte> _texture;
    [JsonIgnore] public ITexture Texture => _texture;
    public SimpleTileGraphics(IReadOnlyTexture<byte> texture) => _texture = texture ?? throw new ArgumentNullException(nameof(texture));
    public Region GetRegion(int imageNumber, int paletteFrame) => _texture.Regions[imageNumber];
    public uint GetDayRegionId(int imageNumber) => (uint)imageNumber;
    public uint GetNightRegionId(int imageNumber) => 0;
    public uint GetPaletteFrameCount(int imageNumber) => 1;
    public bool IsPaletteAnimated(int imageNumber) => false;
}