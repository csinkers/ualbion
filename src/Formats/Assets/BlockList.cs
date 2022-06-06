using System;
using System.Collections.Generic;
using System.Globalization;
using SerdesNet;

#pragma warning disable CA1710 // Identifiers should have correct suffix
namespace UAlbion.Formats.Assets;

public class BlockList : List<Block>
{
    public const int MaxCount = 4095;
    public static BlockList Serdes(int blockNumber, BlockList blockList, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        blockList ??= new BlockList();
        if (s.IsCommenting())
            s.Begin(blockNumber.ToString(CultureInfo.InvariantCulture));

        if (s.IsReading())
        {
            int j = 0;
            while (s.BytesRemaining > 0)
            {
                blockList.Add(Block.Serdes(j, null, s));
                j++;
            }
        }
        else
        {
            s.List(null, blockList, blockList.Count, Block.Serdes);
        }

        if (s.IsCommenting())
            s.End();
        return blockList;
    }
}
#pragma warning restore CA1710 // Identifiers should have correct suffix
