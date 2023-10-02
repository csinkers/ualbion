using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public interface IMapData : IEventSet
{
    MapFlags Flags { get; }
    MapType MapType { get; }
    SongId SongId { get; }
    int Width { get;  }
    int Height { get;  }
    CombatBackgroundId CombatBackgroundId { get;  }
    PaletteId PaletteId { get;  }

    List<MapNpc> Npcs { get; }
    HashSet<ushort> UniqueZoneNodeIds { get; }
}