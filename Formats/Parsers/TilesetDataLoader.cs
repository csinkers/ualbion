using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Tileset)]
    public class TilesetDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var td = new TilesetData();
            td.UseSmallGraphics = config.UseSmallGraphics ?? false;

            var validPassabilities = typeof(Passability).GetEnumValues().Cast<int>().ToList();
            var validLayers = typeof(TileLayer).GetEnumValues().Cast<byte>().ToList();
            var validTypes = typeof(TileType).GetEnumValues().Cast<byte>().ToList();

            int tileCount = (int)(streamLength / 8) + 2;
            td.Tiles.Add(new TileData
            {
                Layer = TileLayer.Normal,
                Type = TileType.Normal,
                Collision = Passability.Passable,
                Flags = 0,
                ImageNumber = 0xffff,
                FrameCount = 1,
                Unk7 = 0
            });

            td.Tiles.Add(new TileData
            {
                Layer = TileLayer.Normal,
                Type = TileType.Normal,
                Collision = Passability.Passable,
                Flags = 0,
                ImageNumber = 0xffff,
                FrameCount = 1,
                Unk7 = 0
            });

            for (int i = 2; i < tileCount; i++)
            {
                var t = new TileData { TileNumber = i };

                byte firstByte = br.ReadByte(); // 0
                t.Layer = (TileLayer)(firstByte >> 4); // Upper nibble of first byte (0h)
                ApiUtil.Assert(validLayers.Contains((byte)t.Layer), "Unexpected tile layer found");

                t.Type = (TileType)(firstByte & 0xf); // Lower nibble of first byte (0l)
                ApiUtil.Assert(validTypes.Contains((byte)t.Type), "Unexpected tile type found");

                t.Collision = (Passability)br.ReadByte(); // 1
                ApiUtil.Assert(validPassabilities.Contains((int)t.Collision));

                t.Flags = (TileFlags)br.ReadUInt16(); // 2
                ApiUtil.Assert((t.Flags & TileFlags.UnusedMask) == 0, "Unused flags set");
                t.ImageNumber = br.ReadUInt16(); // 4
                t.FrameCount = br.ReadByte(); // 6
                t.Unk7 = br.ReadByte(); // 7

                td.Tiles.Add(t);
            }

            return td;
        }
    }
}
