using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Save
{
    public class MapChangeList : List<MapChange>
    {
        public enum ChunkType
        {
            MysterySmall = 0x3,
            Xld = 0x11,
            Mystery6Byte = 0xc8,
        }

        public static MapChangeList Serdes(int _, MapChangeList c, ISerializer s)
        {
            c ??= new MapChangeList();
            uint size = s.UInt32("Size", (uint)(c.Count * MapChange.SizeOnDisk));
            ushort count = s.UInt16(nameof(Count), (ushort)c.Count);
            ApiUtil.Assert(count * MapChange.SizeOnDisk == size - 2);
            for (int i = 0; i < count; i++)
            {
                if(i < c.Count)
                    c[i] = MapChange.Serdes(c[i], s);
                else
                    c.Add(MapChange.Serdes(null, s));
            }

            return c;
        }
    }
}
