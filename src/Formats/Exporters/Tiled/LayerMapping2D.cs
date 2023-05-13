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
        if (map == null) throw new ArgumentNullException(nameof(map));
        var underlay = LoadLayer(map, LayerName.Underlay);
        var overlay = LoadLayer(map, LayerName.Overlay);
        return MapTile.FromInts(underlay, overlay);
    }

    public static List<TiledMapLayer> BuildMapLayers(MapData2D map, TilesetData tileset, ref int nextLayerId)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        var encoding = UncompressedCsvTileEncoding.Instance;

        var underlayId = nextLayerId++;
        var overlayId = nextLayerId++;
        return new()
        {
            BuildLayer(encoding, underlayId, LayerName.Underlay, map.Width, map.Height, TilesToInts(map.Tiles, tileset, false)),
            BuildLayer(encoding, overlayId, LayerName.Overlay, map.Width, map.Height, TilesToInts(map.Tiles, tileset, true))
        };
    }

    static TiledMapLayer BuildLayer(ITileEncoding encoding, int id, string name, int width, int height, int[] data)
    {
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));
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

    static int[] LoadLayer(Map map, string name)
    {
        var layer = map.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (layer == null)
            throw new FormatException($"Expected to find a layer named \"{name}\" in map");

        var tileEncoding = TileEncodings.TryGetEncoding(layer.Data.Encoding, layer.Data.Compression);
        if (tileEncoding == null)
            throw new NotSupportedException($"No encoder could be found for encoding \"{layer.Data.Encoding}\" and compression \"{layer.Data.Compression}\"");

        return tileEncoding.Decode(layer.Data.Content);
    }
}