using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class LayerMapping3D
{
    const int TilesetSpacing = 1000;
    public const int FloorGid = 0;
    public const int WallGid = TilesetSpacing;
    public const int ContentsGid = 2 * TilesetSpacing;
    public const int CeilingGid = 3 * TilesetSpacing;

    public static class LayerName
    {
        public const string Floors = "Floors";
        public const string Ceilings = "Ceilings";
        public const string Walls = "Walls";
        public const string Contents = "Contents";
    }

    public static List<TiledMapLayer> BuildLayers(MapData3D map, ref int nextLayerId)
    {
        int floorId = nextLayerId++;
        int wallId = nextLayerId++;
        int contentId = nextLayerId++;
        int ceilingid = nextLayerId++;
        var encoding = UncompressedCsvTileEncoding.Instance;

        return new List<TiledMapLayer>
        {
            new()
            {
                Id = floorId,
                Name = LayerName.Floors,
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = EncodeLayer(map, IsometricMode.Floors, encoding) }
            },
            new()
            {
                Id = wallId,
                Name = LayerName.Walls,
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = EncodeLayer(map, IsometricMode.Walls, encoding) }
            },
            new()
            {
                Id = contentId,
                Name = LayerName.Contents,
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = EncodeLayer(map, IsometricMode.Contents, encoding) }
            },
            new()
            {
                Id = ceilingid,
                Name = LayerName.Ceilings,
                Width = map.Width,
                Height = map.Height,
                Opacity = 0.5,
                Data = new LayerData { Encoding = "csv", Content = EncodeLayer(map, IsometricMode.Ceilings, encoding) }
            }
        };
    }

    public static void ReadLayers(MapData3D albionMap, List<TiledMapLayer> layers)
    {
        foreach (var layer in layers)
        {
            var tileEncoding = TileEncodings.TryGetEncoding(layer.Data.Encoding, layer.Data.Compression);
            if (tileEncoding == null)
                throw new NotSupportedException($"No encoder could be found for encoding \"{layer.Data.Encoding}\" and compression \"{layer.Data.Compression}\" in layer {layer.Name}");

            var values = tileEncoding.Decode(layer.Data.Content);
            if (values.Length != albionMap.Width * albionMap.Height)
                throw new FormatException($"Map layer {layer.Id} had {values.Length}, but {albionMap.Width * albionMap.Height} were expected ({albionMap.Width} x {albionMap.Height})");

            for (int i = 0; i < values.Length; i++)
            {
                var globalValue = values[i];
                if (globalValue == 0)
                    continue;

                var (type, value) = InterpretTile(globalValue);
                var array = type switch
                {
                    IsometricMode.Floors => albionMap.Floors,
                    IsometricMode.Ceilings => albionMap.Ceilings,
                    IsometricMode.Contents => albionMap.Contents,
                    _ => throw new InvalidOperationException($"Unexpected isometric mode \"{type}\"")
                };

                array[i] = value;
            }
        }
    }

    static (IsometricMode mode, byte value) InterpretTile(int tile) =>
        tile switch
        {
            >= CeilingGid and < CeilingGid + TilesetSpacing => (IsometricMode.Ceilings, (byte)(tile - CeilingGid)),
            >= ContentsGid => (IsometricMode.Contents, (byte)(tile - ContentsGid)),
            >= WallGid => (IsometricMode.Contents, (byte)(LabyrinthData.WallOffset + tile - 1 - WallGid)),
            >= FloorGid => (IsometricMode.Floors, (byte)(tile - FloorGid)),
            _ => throw new ArgumentOutOfRangeException($"Tile {tile} did not fall into any of the expected ranges")
        };

    static string EncodeLayer(MapData3D map, IsometricMode mode, ITileEncoding encoding)
    {
        var (gidOffset, tiles) = mode switch
        {
            IsometricMode.Floors => (FloorGid, map.Floors),
            IsometricMode.Walls => (WallGid, map.BuildWallArray()),
            IsometricMode.Contents => (ContentsGid, map.BuildObjectArray()),
            IsometricMode.Ceilings => (CeilingGid, map.Ceilings),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        var indices = new int[map.Width * map.Height];
        for (int i = 0; i < indices.Length; i++)
        {
            int tile = tiles[i];
            if (tile == 0)
                indices[i] = 0;

            else
                indices[i] = gidOffset + tiles[i];
        }

        return encoding.Encode(indices, map.Width);
    }
}
