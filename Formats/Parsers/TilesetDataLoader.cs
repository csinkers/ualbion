using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.IconData)]
    public class TilesetDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var td = new TilesetData();
            td.UseSmallGraphics = config.UseSmallGraphics ?? false;
            td.Tiles.Add(new TilesetData.TileData
            {
                TileId = 0,
                Layer = TilesetData.TileLayer.Normal,
                Type = 0,
                Collision = 0,
                Flags = (TilesetData.TileFlags)0,
                TileNumber = 0,
                FrameCount = 1,
                Unk7 = 0,
            });

            var validPassabilities = typeof(TilesetData.Passability).GetEnumValues().Cast<int>().ToList();
            var validLayers = typeof(TilesetData.TileLayer).GetEnumValues().Cast<byte>().ToList();
            var validTypes = typeof(TilesetData.TileType).GetEnumValues().Cast<byte>().ToList();

            var overrides = (config.FrameCountOverrides ?? "")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var parts = x.Split(':');
                    int tileNumber = int.Parse(parts[0]) - 1;
                    int frameCount = int.Parse(parts[1]);
                    return (tileNumber, frameCount);
                }).ToDictionary(x => x.tileNumber, x => (byte)x.frameCount);

            int tileCount = (int)(streamLength / 8);
            for (int i = 0; i < tileCount; i++)
            {
                var t = new TilesetData.TileData { TileId = i + 1 };

                byte firstByte = br.ReadByte(); // 0
                t.Layer = (TilesetData.TileLayer)(firstByte >> 4); // Upper nibble of first byte (0h)
                Debug.Assert(validLayers.Contains((byte)t.Layer), "Unexpected tile layer found");

                t.Type = (TilesetData.TileType)(firstByte & 0xf); // Lower nibble of first byte (0l)
                Debug.Assert(validTypes.Contains((byte)t.Type), "Unexpected tile type found");

                t.Collision = (TilesetData.Passability)br.ReadByte(); // 1
                Debug.Assert(validPassabilities.Contains((int)t.Collision));

                t.Flags = (TilesetData.TileFlags)br.ReadUInt16(); // 2
                Debug.Assert((t.Flags & TilesetData.TileFlags.UnusedMask) == 0, "Unused flags set");
                t.TileNumber = (ushort)(br.ReadUInt16() - 1); // 4
                t.FrameCount = br.ReadByte(); // 6
                t.Unk7 = br.ReadByte(); // 7

                if (overrides.ContainsKey(i))
                {
                    t.FrameCount = overrides[i];
                    if(t.FrameCount == 0)
                    {
                        t.TileNumber = 0;
                        t.FrameCount = 1;
                    }
                }

                td.Tiles.Add(t);
            }

            return td;
        }
    }
}