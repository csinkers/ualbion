using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save
{
    public class MapChangeList : List<MapChange>
    {
        public static MapChangeList Serdes(int _, MapChangeList c, ISerializer s)
        {
            c ??= new MapChangeList();
            s.Begin();
            uint size = s.UInt32("Size", (uint)(c.Count * MapChange.SizeOnDisk + 2));
            ushort count = s.UInt16(nameof(Count), (ushort)c.Count);
            ApiUtil.Assert(count * MapChange.SizeOnDisk == size - 2);
            for (int i = 0; i < count; i++)
            {
                if(i < c.Count)
                    c[i] = MapChange.Serdes(i, c[i], s);
                else
                    c.Add(MapChange.Serdes(i, null, s));
            }

            s.End();
            return c;
        }

        public void Update(MapDataId mapId, byte x, byte y, IconChangeType type, ushort value)
        {
            var change = this.FirstOrDefault(c =>
                c.MapId == mapId &&
                c.X == x &&
                c.Y == y &&
                c.ChangeType == type);

            if (change == null)
            {
                change = new MapChange { MapId = mapId, X = x, Y = y, ChangeType = type };
                Add(change);
            }

            change.Value = value;
        }
    }
}
