using System;
using System.IO;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public class TiledTilesetLoader : Component, IAssetLoader<TilesetData>
{
    public static readonly StringAssetProperty BlankTilePathProperty = new("BlankTilePath"); 
    public static readonly PathPatternProperty GraphicsPattern = new("GraphicsPattern"); 
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((TilesetData)existing, s, context);

    public TilesetData Serdes(TilesetData existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);

        var graphicsTemplate = context.GetProperty(GraphicsPattern, AssetPathPattern.Build("{0}/{0}_{1}.png"));
        var blankTilePath = context.GetProperty(BlankTilePathProperty, "Blank.png");

        var properties = new Tilemap2DProperties
        {
            GraphicsTemplate = graphicsTemplate,
            BlankTilePath = blankTilePath,
            TileWidth = 16,
            TileHeight = 16
        };

        return s.IsWriting() 
            ? Save(existing, properties, s) 
            : Load(context, properties, s);
    }

    static TilesetData Load(AssetLoadContext context, Tilemap2DProperties properties, ISerdes serdes)
    {
        var xmlBytes = serdes.Bytes(null, null, (int)serdes.BytesRemaining);
        using var ms = new MemoryStream(xmlBytes);
        var tileset = Tileset.Parse(ms);
        return TilesetMapping.ToAlbion(tileset, context.AssetId, properties);
    }

    static TilesetData Save(TilesetData tileset, Tilemap2DProperties properties, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(tileset);
        var tiledTileset = TilesetMapping.FromAlbion(tileset, properties);
        var bytes = FormatUtil.BytesFromTextWriter(tiledTileset.Serialize);
        s.Bytes(null, bytes, bytes.Length);
        return tileset;
    }
}