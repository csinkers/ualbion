using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Maps
{
    public abstract class BaseMapData : IMapData
    {
        public MapDataId Id { get; }
        public abstract MapType MapType { get; }
        public byte Width { get; protected set; }
        public byte Height { get; protected set; }
        public SongId? SongId { get; protected set; }
        public PaletteId PaletteId { get; protected set; }
        public CombatBackgroundId CombatBackgroundId { get; protected set; }

        public IDictionary<int, MapNpc> Npcs { get; } = new Dictionary<int, MapNpc>();

        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public IDictionary<int, MapEventZone> ZoneLookup { get; } = new Dictionary<int, MapEventZone>();
        public IDictionary<TriggerTypes, MapEventZone[]> ZoneTypeLookup { get; } = new Dictionary<TriggerTypes, MapEventZone[]>();
        public IList<EventNode> Events { get; } = new List<EventNode>();
        public IList<EventChain> Chains { get; } = new List<EventChain>();
        public (EventChain, IEventNode)[] ChainsByEventId { get; private set; }
#if DEBUG
        public IList<object>[] EventReferences { get; private set; }
#endif

        protected BaseMapData(MapDataId id) { Id = id; }

        protected void SerdesZones(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            int zoneCount = s.UInt16("ZoneCount", (ushort)Zones.Count(x => x.Global));
            s.List(nameof(Zones), Zones, zoneCount, (i, x, serializer) => MapEventZone.Serdes(x, serializer, 0xff));
            s.Check();

            int zoneOffset = zoneCount;
            for (byte y = 0; y < Height; y++)
            {
                zoneCount = s.UInt16("RowZones", (ushort)Zones.Count(x => x.Y == y && !x.Global));
                var y1 = y;
                s.List(nameof(Zones), Zones, zoneCount, zoneOffset, (i, x, s2) => MapEventZone.Serdes(x, s2, y1));
                zoneOffset += zoneCount;
            }
        }

        protected void SerdesEvents(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            ushort eventCount = s.UInt16("EventCount", (ushort)Events.Count);

            if(Events != null) // Ensure ids match up
                for (ushort i = 0; i < Events.Count; i++)
                    Events[i].Id = i;

            s.List(nameof(Events), Events, eventCount, (i, x, serializer) 
                => EventNode.Serdes((ushort)i, x, serializer, false, (ushort)Id));

            foreach (var node in Events)
                node.Unswizzle(Events);

            s.Check();
        }

        protected void SerdesChains(ISerializer s, int chainCount)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var chainOffsets = Chains.Select(x => x.Events[0].Id).ToList();
            for(int i = 0; i < chainCount; i++)
            {
                if(s.Mode == SerializerMode.Reading)
                {
                    var eventId = s.UInt16(null, 0);
                    if(eventId != 0xffff)
                        chainOffsets.Add(eventId);
                }
                else
                {
                    var eventId = chainOffsets.Count <= i ? (ushort)0xffff : chainOffsets[i];
                    s.UInt16(null, eventId);
                }
            }

            if(s.Mode == SerializerMode.Reading)
            {
                ChainsByEventId = new (EventChain, IEventNode)[Events.Count];
                for (int i = 0; i < chainOffsets.Count; i++)
                {
                    var offset = chainOffsets[i];
                    var chain = new EventChain(i);
                    var nextOffset = chainOffsets.Count == i + 1
                        ? Events.Count
                        : chainOffsets[i + 1];

                    for (int j = offset; j < nextOffset; j++)
                    {
                        chain.Events.Add(Events[j]);
                        ChainsByEventId[j] = (chain, Events[j]);
                    }

                    Chains.Add(chain);
                }
            }

            s.Check();
        }

        protected void Unswizzle() // Resolve event indices to pointers
        {
            // Use map events if the event number is set, otherwise use the event set from the NPC's character sheet.
            // Note: Event set loading requires IAssetManager, so can't be done directly by UAlbion.Formats code.
            // Instead, the MapManager will call AttachEventSets with a callback to load the event sets.
            foreach (var npc in Npcs.Values)
                npc.Unswizzle(x => ChainsByEventId[x]);

            foreach (var zone in Zones)
                zone.Unswizzle(x => ChainsByEventId[x]);

            foreach (var position in Zones.Where(x => !x.Global).GroupBy(x => x.Y * Width + x.X))
            {
                var zone = position.SingleOrDefault();
                if (zone != null)
                    ZoneLookup[position.Key] = zone;
            }

            foreach (var triggerType in Zones.GroupBy(x => x.Trigger))
                ZoneTypeLookup[triggerType.Key] = triggerType.ToArray();

#if DEBUG
            EventReferences = new IList<object>[Events.Count];
            foreach (var zone in Zones)
                if (zone.Node != null)
                    AddEventReference(zone.Node.Id, zone);

            foreach (var npc in Npcs.Values)
                if (npc.Node != null)
                    AddEventReference(npc.Node.Id, npc);

            foreach (var e in Events)
            {
                if (e.Next != null)
                    AddEventReference(e.Next.Id, e);

                if (e is BranchNode branch && branch.NextIfFalse != null)
                    AddEventReference(branch.NextIfFalse.Id, e);
            }
#endif
        }

#if DEBUG
        void AddEventReference(int id, object referrer)
        {
            if (EventReferences[id] == null)
                EventReferences[id] = new List<object>();
            EventReferences[id].Add(referrer);
        }
#endif

        protected void SerdesNpcWaypoints(ISerializer s)
        {
            foreach (var npc in Npcs.OrderBy(x => x.Key).Select(x => x.Value))
            {
                if (npc.Id.HasValue)
                    npc.LoadWaypoints(s);
                else 
                    npc.Waypoints = new NpcWaypoint[1];
            }
        }

        public void AttachEventSets(Func<NpcCharacterId, ICharacterSheet> characterSheetLoader, Func<EventSetId, EventSet> eventSetLoader)
        {
            // Wire up event sets for NPCs that don't have map-specific events.
            foreach (var npc in Npcs.Values)
            {
                if (npc.Id == null)
                    continue;

                if (npc.Chain == null)
                {
                    var chain = new EventChain(-1);
                    chain.Events.Add(new EventNode(0, new StartDialogueEvent((NpcCharacterId)npc.Id.Value)));
                    npc.Chain = chain;
                }
            }
        }
    }
}
