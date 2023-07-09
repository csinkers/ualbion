using System;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class InterlacedBitmapLoader : IAssetLoader<IReadOnlyTexture<uint>>
{
    public IReadOnlyTexture<uint> Serdes(IReadOnlyTexture<uint> existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (s.IsWriting()) // TODO: Implement writing if required. May require palette generation, which can get complicated.
            return existing;

        var ilbm = InterlacedBitmap.Serdes(null, s);
        return s.IsWriting() ? existing : ConvertIlbmToTexture(ilbm, context);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<uint>)existing, s, context);

    static IReadOnlyTexture<uint> ConvertIlbmToTexture(InterlacedBitmap bitmap, AssetLoadContext info)
    {
        var texture = new SimpleTexture<uint>(info.AssetId, bitmap.Width, bitmap.Height);
        texture.AddRegion(0, 0, bitmap.Width, bitmap.Height);

        var imageBuffer = new ReadOnlyImageBuffer<byte>(bitmap.Width, bitmap.Height, bitmap.Width, bitmap.ImageData);
        BlitUtil.BlitTiled8To32(imageBuffer, texture.GetMutableRegionBuffer(0), bitmap.Palette, 255, null);
        return texture;
    }
}
