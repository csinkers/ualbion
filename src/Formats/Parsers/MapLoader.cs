using System;
using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MapData)]
    public class MapLoader : IAssetLoader<IMapData>
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
            => Serdes(null, new AlbionReader(br, streamLength), key, config);

        public IMapData Serdes(IMapData existing, ISerializer s, AssetKey key, AssetInfo config)
        {
            var startPosition = s.Offset;
            s.UInt16("DummyRead", 0); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
            MapType mapType = s.EnumU8(nameof(mapType), existing?.MapType ?? MapType.Unknown);
            s.Seek(startPosition);

            return mapType switch
            {
                MapType.TwoD => MapData2D.Serdes((MapData2D)existing, s, config),
                MapType.TwoDOutdoors => MapData2D.Serdes((MapData2D)existing, s, config),
                MapType.ThreeD => MapData3D.Serdes((MapData3D)existing, s, config),
                _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
            };
        }
    }
}
