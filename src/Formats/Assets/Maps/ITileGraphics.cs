using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public interface ITileGraphics
{
    ITexture Texture { get; }
    Region GetRegion(int imageNumber, int paletteFrame);
    uint GetDayRegionId(int imageNumber);
    uint GetNightRegionId(int imageNumber);
    uint GetPaletteFrameCount(int imageNumber);
    bool IsPaletteAnimated(int imageNumber);
}
