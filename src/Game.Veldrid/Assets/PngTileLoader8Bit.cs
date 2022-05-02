using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets;

public class PngTileLoader8Bit : Component, IAssetLoader<ITileGraphics>
{
    readonly Png8Loader _png8Loader = new();

    public ITileGraphics Serdes(ITileGraphics existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        if (s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            if (existing.Texture is not IReadOnlyTexture<byte> texture)
                throw new FormatException(
                    $"Tried to save tileset {info.AssetId} as pngs using PngTileLoader8Bit, but it is not backed by 8-bit textures");

            _png8Loader.Serdes(texture, info, s, context);
            return existing;
        }
        else
        {
            var texture = _png8Loader.Serdes(null, info, s, context);
            texture = AtlasPostProcessor.Process(texture, info);
            return new SimpleTileGraphics(texture);
        }
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((ITileGraphics)existing, info, s, context);
}