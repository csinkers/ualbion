using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using static UAlbion.Formats.Exporters.Tiled.MapperUtil;

namespace UAlbion.Formats.Exporters.Tiled
{
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
            return new List<TiledMapLayer>
            {
                new()
                {
                    Id = floorId,
                    Name = LayerName.Floors,
                    Width = map.Width,
                    Height = map.Height,
                    Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Floors) }
                },
                new()
                {
                    Id = wallId,
                    Name = LayerName.Walls,
                    Width = map.Width,
                    Height = map.Height,
                    Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Walls) }
                },
                new()
                {
                    Id = contentId,
                    Name = LayerName.Contents,
                    Width = map.Width,
                    Height = map.Height,
                    Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Contents) }
                },
                new()
                {
                    Id = ceilingid,
                    Name = LayerName.Ceilings,
                    Width = map.Width,
                    Height = map.Height,
                    Opacity = 0.5,
                    Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Ceilings) }
                }
            };
        }

        public static void ReadLayers(MapData3D albionMap, Map map)
        {
            albionMap.Floors = new byte[map.Width * map.Height];
            albionMap.Ceilings = new byte[map.Width * map.Height];
            albionMap.Contents = new byte[map.Width * map.Height];

            foreach (var layer in map.Layers)
            {
                var values = ParseCsv(layer.Data.Content).ToArray();
                if (values.Length != map.Width * map.Height)
                    throw new FormatException($"Map layer {layer.Id} had {values.Length}, but {map.Width * map.Height} were expected ({map.Width} x {map.Height})");

                for (int i = 0; i < values.Length; i++)
                {
                    var (type, value) = InterpretTile(values[i]);
                    var array = type switch
                        {
                            IsometricMode.Floors => albionMap.Floors,
                            IsometricMode.Ceilings => albionMap.Ceilings,
                            IsometricMode.Contents => albionMap.Contents,
                            _ => throw new ArgumentOutOfRangeException()
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
                >= WallGid => (IsometricMode.Contents, (byte)(tile - WallGid)),
                >= FloorGid => (IsometricMode.Contents, (byte)(LabyrinthData.WallOffset + tile - FloorGid)),
                _ => throw new ArgumentOutOfRangeException($"Tile {tile} did not fall into any of the expected ranges")
            };

        static string BuildCsvData(MapData3D map, IsometricMode mode)
        {
            var (gidOffset, tiles) = mode switch
            {
                IsometricMode.Floors => (FloorGid, map.Floors),
                IsometricMode.Walls => (WallGid, map.BuildWallArray()),
                IsometricMode.Contents => (ContentsGid, map.BuildObjectArray()),
                IsometricMode.Ceilings => (CeilingGid, map.Ceilings),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };

            var sb = new StringBuilder();
            for (int j = 0; j < map.Height; j++)
            {
                for (int i = 0; i < map.Width; i++)
                {
                    int index = j * map.Width + i;
                    sb.Append(gidOffset + tiles[index]);
                    sb.Append(',');
                }

                sb.AppendLine();
            }
            return sb.ToString(0, sb.Length - (Environment.NewLine.Length + 1));
        }
    }
}