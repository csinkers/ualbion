using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public interface ITileGraphics
{
    ITexture Texture { get; }
    Region GetRegion(int imageNumber, int frame, int paletteFrame);
}