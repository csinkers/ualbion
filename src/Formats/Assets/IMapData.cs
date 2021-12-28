using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Assets
{
    public interface IMapData
    {
        MapType MapType { get; }
        SongId SongId { get; }
        byte Width { get;  }
        byte Height { get;  }
        SpriteId CombatBackgroundId { get;  }
        PaletteId PaletteId { get;  }

        MapNpc[] Npcs { get; }
        List<EventNode> Events { get; }
        List<ushort> Chains { get; }
        List<MapEventZone> Zones { get; }
        Dictionary<int, MapEventZone> ZoneLookup { get; }
        Dictionary<TriggerTypes, MapEventZone[]> ZoneTypeLookup { get; }
    }
}
