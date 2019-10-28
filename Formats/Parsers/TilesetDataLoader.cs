using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.IconData)]
    public class TilesetDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var td = new TilesetData();
            td.UseSmallGraphics = config.UseSmallGraphics ?? false;

            var validPassabilities = typeof(TilesetData.Passability).GetEnumValues().Cast<int>().ToList();
            var validLayers = typeof(TilesetData.TileLayer).GetEnumValues().Cast<byte>().ToList();
            var validTypes = typeof(TilesetData.TileType).GetEnumValues().Cast<byte>().ToList();

            int tileCount = (int)(streamLength / 8);
            for (int i = 0; i < tileCount; i++)
            {
                var t = new TilesetData.TileData { TileNumber = i };

                byte firstByte = br.ReadByte(); // 0
                t.Layer = (TilesetData.TileLayer)(firstByte >> 4); // Upper nibble of first byte (0h)
                Debug.Assert(validLayers.Contains((byte)t.Layer), "Unexpected tile layer found");

                t.Type = (TilesetData.TileType)(firstByte & 0xf); // Lower nibble of first byte (0l)
                Debug.Assert(validTypes.Contains((byte)t.Type), "Unexpected tile type found");

                t.Collision = (TilesetData.Passability)br.ReadByte(); // 1
                Debug.Assert(validPassabilities.Contains((int)t.Collision));

                t.Flags = (TilesetData.TileFlags)br.ReadUInt16(); // 2
                Debug.Assert((t.Flags & TilesetData.TileFlags.UnusedMask) == 0, "Unused flags set");
                t.ImageNumber = br.ReadUInt16(); // 4
                t.FrameCount = br.ReadByte(); // 6
                t.Unk7 = br.ReadByte(); // 7

                td.Tiles.Add(t);
            }

            return td;
        }
    }
}