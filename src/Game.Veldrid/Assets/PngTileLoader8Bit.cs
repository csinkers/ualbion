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
    public PngTileLoader8Bit() => AttachChild(_png8Loader);

    public ITileGraphics Serdes(ITileGraphics existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            if (existing.Texture is not IReadOnlyTexture<byte> texture)
                throw new FormatException(
                    $"Tried to save tileset {context.AssetId} as pngs using PngTileLoader8Bit, but it is not backed by 8-bit textures");

            _png8Loader.Serdes(texture, s, context);
            return existing;
        }
        else
        {
            var texture = _png8Loader.Serdes(null, s, context);
            texture = AtlasPostProcessor.Process(texture, context);
            return new SimpleTileGraphics(texture);
        }
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ITileGraphics)existing, s, context);
}