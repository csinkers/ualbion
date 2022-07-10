using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Formats.Assets.Maps;
using static UAlbion.Formats.Exporters.Tiled.MapperUtil;

namespace UAlbion.Formats.Exporters.Tiled;

public static class LayerMapping2D
{
    const ushort BlankTileIndex = 0;
    public static class LayerName
    {
        public const string Underlay = "Underlay";
        public const string Overlay = "Overlay";
    }

    public static List<TiledMapLayer> BuildLayers(MapData2D map, TilesetData tileset, ref int nextLayerId)
    {
        var underlayId = nextLayerId++;
        var overlayId = nextLayerId++;
        return new()
        {
            new()
            {
                Id = underlayId,
                Name = LayerName.Underlay,
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, false) }
            },
            new()
            {
                Id = overlayId,
                Name = LayerName.Overlay,
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, true) }
            }
        };
    }

    static string BuildCsvData(MapData2D map, TilesetData tileset, bool useOverlay)
    {
        var sb = new StringBuilder();
        for (int j = 0; j < map.Height; j++)
        {
            for (int i = 0; i < map.Width; i++)
            {
                int index = j * map.Width + i;
                var tileIndex = useOverlay ? map.Tiles[index].Overlay : map.Tiles[index].Underlay;
                var tile = tileset.Tiles[tileIndex];
                sb.Append(tile.IsBlank ? BlankTileIndex : tileIndex);
                sb.Append(',');
            }

            sb.AppendLine();
        }
        return sb.ToString(0, sb.Length - (Environment.NewLine.Length + 1));
    }

    static int[] LoadLayer(Map map, string name)
    {
        var layer = map.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (layer == null)
            throw new FormatException($"Expected to find a layer named \"{name}\" in map");

        return ParseCsv(layer.Data.Content).ToArray();
    }

    public static MapTile[] ReadLayout(Map map)
    {
        var underlay = LoadLayer(map, LayerName.Underlay);
        var overlay = LoadLayer(map, LayerName.Overlay);
        return MapTile.FromInts(underlay, overlay);
    }
}