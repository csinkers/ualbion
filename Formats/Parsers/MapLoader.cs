using System;
using System.IO;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.MapData)]
    public class MapLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var startPosition = br.BaseStream.Position;
            br.ReadUInt16(); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
            int mapType = br.ReadByte();
            br.BaseStream.Position = startPosition;
            switch (mapType)
            {
                case 1: return MapData3D.Load(br, streamLength, name);
                case 2: return MapData2D.Load(br, streamLength, name);
                default: throw new NotImplementedException($"Unrecognised map type {mapType} found.");
            }
        }
    }
}