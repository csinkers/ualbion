using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;

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

        IDictionary<int, MapNpc> Npcs { get; }
        IList<EventNode> Events { get; }
        IList<EventChain> Chains { get; }
        IList<MapEventZone> Zones { get; }
        IDictionary<int, MapEventZone> ZoneLookup { get; }
        IDictionary<TriggerTypes, MapEventZone[]> ZoneTypeLookup { get; }
        void AttachEventSets(Func<NpcId, ICharacterSheet> characterSheetLoader, Func<EventSetId, EventSet> eventSetLoader);
    }
}
