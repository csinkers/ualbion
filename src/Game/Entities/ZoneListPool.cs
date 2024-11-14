using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities;

public class ZoneListPool : Pooled<List<MapEventZone>>
{
    public static readonly ZoneListPool Shared = new();
    ZoneListPool() : base(() => [], x => x.Clear()) { }
}