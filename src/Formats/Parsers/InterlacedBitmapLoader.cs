using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class InterlacedBitmapLoader : IAssetLoader<IReadOnlyTexture<uint>>
{
    public IReadOnlyTexture<uint> Serdes(IReadOnlyTexture<uint> existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s.IsWriting()) // TODO: Implement writing if required. May require palette generation, which can get complicated.
            return existing;

        var ilbm = InterlacedBitmap.Serdes(null, s);
        return s.IsWriting() ? existing : ConvertIlbmToTexture(ilbm, info);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((IReadOnlyTexture<uint>)existing, info, s, context);

    static IReadOnlyTexture<uint> ConvertIlbmToTexture(InterlacedBitmap bitmap, AssetInfo info)
    {
        var texture =
            new SimpleTexture<uint>(info.AssetId, bitmap.Width, bitmap.Height)
            .AddRegion(0, 0, bitmap.Width, bitmap.Height);

        var imageBuffer = new ReadOnlyImageBuffer<byte>(bitmap.Width, bitmap.Height, bitmap.Width, bitmap.ImageData);
        BlitUtil.BlitTiled8To32(imageBuffer, texture.GetMutableRegionBuffer(0), bitmap.Palette, 255, null);
        return texture;
    }
}
