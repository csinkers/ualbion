using System;
using System.Collections.Generic;
using SerdesNet;

#pragma warning disable CA1710 // Identifiers should have correct suffix
namespace UAlbion.Formats.Assets
{
    public class BlockList : List<Block>
    {
        public static BlockList Serdes(int _, BlockList blockList, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            blockList ??= new BlockList();
            if (s.IsReading())
            {
                int j = 0;
                while (!s.IsComplete())
                {
                    blockList.Add(Block.Serdes(j, null, s));
                    j++;
                }
            }
            else
            {
                s.List(null, blockList, blockList.Count, Block.Serdes);
            }

            return blockList;
        }
    }
} 
#pragma warning restore CA1710 // Identifiers should have correct suffix
