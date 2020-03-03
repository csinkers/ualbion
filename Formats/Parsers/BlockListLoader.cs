using System.Collections.Generic;
using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.BlockList)]
    public class BlockListLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var initial = br.BaseStream.Position;

            var blockList = new List<Block>();
            while (br.BaseStream.Position < initial + streamLength)
            {
                var block = new Block();
                block.Width = br.ReadByte();
                block.Height = br.ReadByte();
                block.Underlay = new int[block.Width * block.Height];
                block.Overlay = new int[block.Width * block.Height];

                for (int i = 0; i < block.Underlay.Length; i++)
                {
                    byte b1 = br.ReadByte();
                    byte b2 = br.ReadByte();
                    byte b3 = br.ReadByte();

                    int underlay = ((b2 & 0x0F) << 8) + b3;
                    int overlay = (b1 << 4) + (b2 >> 4);
                    block.Underlay[i] = underlay; 
                    block.Overlay[i] = overlay;
                }
                blockList.Add(block);
            }

            return blockList;
        }
    }
}
