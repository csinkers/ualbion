using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save;

public class MapChangeCollection : List<MapChange>
{
    public static MapChangeCollection Serdes(int _, MapChangeCollection c, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        c ??= [];
        uint size = s.UInt32("Size", (uint)(c.Count * MapChange.SizeOnDisk + 2));
        ushort count = s.UInt16(nameof(Count), (ushort)c.Count);
        ApiUtil.Assert(count * MapChange.SizeOnDisk == size - 2);
        for (int i = 0; i < count; i++)
        {
            if(i < c.Count)
                c[i] = MapChange.Serdes(i, c[i], mapping, s);
            else
                c.Add(MapChange.Serdes(i, null, mapping, s));
        }

        return c;
    }

    public void Update(MapId mapId, byte x, byte y, IconChangeType type, ushort value)
    {
        foreach (var c in this)
        {
            if (c.MapId != mapId || c.X != x || c.Y != y || c.ChangeType != type) 
                continue;

            c.Value = value;
            return;
        }

        Add(new MapChange { MapId = mapId, X = x, Y = y, ChangeType = type });
    }
}