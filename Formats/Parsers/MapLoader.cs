using System;
using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MapData)]
    public class MapLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var startPosition = br.BaseStream.Position;
            br.ReadUInt16(); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
            var mapType = (MapType)br.ReadByte();
            br.BaseStream.Position = startPosition;
            return mapType switch
            {
                MapType.TwoD   => MapData2D.Load(br, streamLength, name),
                MapType.ThreeD => MapData3D.Load(br, streamLength, name),
                _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
            };
        }

        public object Serdes(object existing, ISerializer s, string name, AssetInfo config)
        {
            var startPosition = s.Offset;
            s.UInt16("DummyRead", 0); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
            MapType mapType = s.EnumU8(nameof(mapType), ((IMapData)existing)?.MapType ?? MapType.Unknown);
            s.Seek(startPosition);

            return mapType switch
            {
                MapType.TwoD => (IMapData)MapData2D.Serdes((MapData2D)existing, s, name, config),
                MapType.ThreeD => MapData3D.Serdes((MapData3D)existing, s, name, config),
                _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
            };
        }
    }
}
