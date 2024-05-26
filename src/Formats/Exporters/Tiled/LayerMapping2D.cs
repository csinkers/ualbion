using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class LayerMapping2D
{
    const ushort BlankTileIndex = 0;
    static class LayerName
    {
        public const string Underlay = "Underlay";
        public const string Overlay = "Overlay";
    }

    public static MapTile[] ReadMapLayers(Map map)
    {
        ArgumentNullException.ThrowIfNull(map);
        var firstGid = map.Tilesets[0].FirstGid; // Assume the main tileset always comes first
        var underlay = LoadLayer(map, LayerName.Underlay, firstGid);
        var overlay = LoadLayer(map, LayerName.Overlay, firstGid);
        return MapTile.FromInts(underlay, overlay);
    }

    public static List<TiledMapLayer> BuildMapLayers(MapData2D map, TilesetData tileset, ref int nextLayerId)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(tileset);
        var encoding = UncompressedCsvTileEncoding.Instance;

        var underlayId = nextLayerId++;
        var overlayId = nextLayerId++;
        return new()
        {
            BuildLayer(encoding, underlayId, LayerName.Underlay, map.Width, map.Height, TilesToInts(map.Tiles, tileset, false)),
            BuildLayer(encoding, overlayId, LayerName.Overlay, map.Width, map.Height, TilesToInts(map.Tiles, tileset, true))
        };
    }

    static TiledMapLayer BuildLayer(UncompressedCsvTileEncoding encoding, int id, string name, int width, int height, int[] data)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        return new()
        {
            Id = id,
            Name = name,
            Width = width,
            Height = height,
            Data = new LayerData
            {
                Encoding = encoding.Encoding,
                Compression = encoding.Compression,
                Content = encoding.Encode(data, width)
            }
        };
    }

    static int[] TilesToInts(Span<MapTile> tiles, TilesetData tileset, bool useOverlay)
    {
        var result = new int[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            var tileIndex = useOverlay ? tiles[i].Overlay : tiles[i].Underlay;
            var tile = tileset.Tiles[tileIndex];
            result[i] = tile.IsBlank ? BlankTileIndex : tileIndex;
        }

        return result;
    }

    static int[] LoadLayer(Map map, string name, int firstGid)
    {
        var layer = map.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (layer == null)
            throw new FormatException($"Expected to find a layer named \"{name}\" in map");

        var tileEncoding = TileEncodings.TryGetEncoding(layer.Data.Encoding, layer.Data.Compression);
        if (tileEncoding == null)
            throw new NotSupportedException($"No encoder could be found for encoding \"{layer.Data.Encoding}\" and compression \"{layer.Data.Compression}\"");

        var tiles = tileEncoding.Decode(layer.Data.Content);
        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = tiles[i];
            if (tile >= firstGid)
                tiles[i] = tile - firstGid;
        }

        return tiles;
    }
}