using System;
using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MapData)]
    public class MapLoader : IAssetLoader<IMapData>
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            /* Uncomment for diffing between old and new serialisation methods.
            var startPosition = br.BaseStream.Position;
            br.ReadUInt16(); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
            var mapType = (MapType)br.ReadByte();

            br.BaseStream.Position = startPosition;
            IMapData oldMap = mapType switch
            {
                MapType.TwoD   => MapData2D.Load(br, streamLength, name),
                MapType.ThreeD => MapData3D.Load(br, streamLength, name),
                _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
            };
            br.BaseStream.Position = startPosition; */
            var newMap = Serdes(null, new GenericBinaryReader(br, streamLength), name, config);

            /* Uncomment for diffing between old and new serialisation methods.
            string oldText;
            using(var ms = new MemoryStream())
            {
                using var sw = new StreamWriter(ms);
                var writer = new AnnotatedFormatWriter(sw);
                Serdes(oldMap, writer, "Old", config);
                oldText = Encoding.ASCII.GetString(ms.ToArray());
            }

            string newText;
            using(var ms = new MemoryStream())
            {
                using var sw = new StreamWriter(ms);
                var writer = new AnnotatedFormatWriter(sw);
                Serdes(newMap, writer, "New", config);
                newText = Encoding.ASCII.GetString(ms.ToArray());
            }

            Debug.Assert(oldText == newText);
            */

            return newMap;
        }

        public IMapData Serdes(IMapData existing, ISerializer s, string name, AssetInfo config)
        {
            var startPosition = s.Offset;
            s.UInt16("DummyRead", 0); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
            MapType mapType = s.EnumU8(nameof(mapType), existing?.MapType ?? MapType.Unknown);
            s.Seek(startPosition);

            return mapType switch
            {
                MapType.TwoD => MapData2D.Serdes((MapData2D)existing, s),
                MapType.ThreeD => MapData3D.Serdes((MapData3D)existing, s, name, config),
                _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
            };
        }
    }
}
