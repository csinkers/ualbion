using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Veldrid.Assets;

public class PngTileLoader32Bit : Component, IAssetLoader<ITileGraphics>
{
    public ITileGraphics Serdes(ITileGraphics existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s.IsWriting())
            throw new NotSupportedException("Saving png tile graphics is not currently supported");

        throw new NotImplementedException();
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((ITileGraphics)existing, info, s, context);
}
