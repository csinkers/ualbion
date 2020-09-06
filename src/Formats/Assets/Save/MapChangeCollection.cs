﻿using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save
{
    public class MapChangeCollection : List<MapChange>
    {
        public static MapChangeCollection Serdes(int _, MapChangeCollection c, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            c ??= new MapChangeCollection();
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
