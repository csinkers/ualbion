using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public interface IMapData
{
    MapFlags Flags { get; }
    MapType MapType { get; }
    SongId SongId { get; }
    byte Width { get;  }
    byte Height { get;  }
    SpriteId CombatBackgroundId { get;  }
    PaletteId PaletteId { get;  }

    List<MapNpc> Npcs { get; }
    List<EventNode> Events { get; }
    List<ushort> Chains { get; }
    HashSet<ushort> UniqueZoneNodeIds { get; }
}