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
        IList<EventNode> Events { get; }
        IList<ushort> Chains { get; }
        IList<MapEventZone> Zones { get; }
        IDictionary<int, MapEventZone> ZoneLookup { get; }
        IDictionary<TriggerTypes, MapEventZone[]> ZoneTypeLookup { get; }
    }
}
