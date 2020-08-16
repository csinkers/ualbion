using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.BlockList)]
    public class BlockListLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            var initial = br.BaseStream.Position;

            var blockList = new List<Block>();
            while (br.BaseStream.Position < initial + streamLength)
            {
                byte width = br.ReadByte();
                byte height = br.ReadByte();
                int[] underlays = new int[width * height];
                int[] overlays = new int[width * height];

                for (int i = 0; i < underlays.Length; i++)
                {
                    byte b1 = br.ReadByte();
                    byte b2 = br.ReadByte();
                    byte b3 = br.ReadByte();

                    int underlay = ((b2 & 0x0F) << 8) + b3;
                    int overlay = (b1 << 4) + (b2 >> 4);
                    underlays[i] = underlay;
                    overlays[i] = overlay;
                }

                blockList.Add( new Block(width, height, underlays, overlays));
            }

            return blockList;
        }
    }
}
