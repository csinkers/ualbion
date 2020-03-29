using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public interface IMapData
    {
        MapType MapType { get; }
        SongId? SongId { get; }
        byte Width { get;  }
        byte Height { get;  }
        CombatBackgroundId CombatBackgroundId { get;  }
        PaletteId PaletteId { get;  }

        IDictionary<int, MapNpc> Npcs { get; }
        IList<EventNode> Events { get; }
        IList<EventChain> Chains { get; }
        IList<MapEventZone> Zones { get; }
        IDictionary<int, MapEventZone> ZoneLookup { get; }
        IDictionary<TriggerType, MapEventZone[]> ZoneTypeLookup { get; }
        void AttachEventSets(Func<NpcCharacterId, ICharacterSheet> characterSheetLoader, Func<EventSetId, EventSet> eventSetLoader);
    }
}
