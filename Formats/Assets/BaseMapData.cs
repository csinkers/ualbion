using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public abstract class BaseMapData : IMapData
    {
        public abstract MapType MapType { get; }
        public byte Width { get; protected set; }
        public byte Height { get; protected set; }
        public SongId? SongId { get; protected set; }
        public PaletteId PaletteId { get; protected set; }
        public CombatBackgroundId CombatBackgroundId { get; protected set; }

        public IList<MapNpc> Npcs { get; } = new List<MapNpc>();

        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public IDictionary<int, MapEventZone> ZoneLookup { get; } = new Dictionary<int, MapEventZone>(); 
        public IDictionary<TriggerType, MapEventZone[]> ZoneTypeLookup { get; } = new Dictionary<TriggerType, MapEventZone[]>();
        public IList<EventNode> Events { get; } = new List<EventNode>();
        public IList<EventChain> Chains { get; } = new List<EventChain>();
        public (EventChain, IEventNode)[] ChainsByEventId { get; private set; }
#if DEBUG
        public IList<object>[] EventReferences { get; private set; }
#endif

        /*void BuildEventChains()
        {
            ChainsByEventId = new (EventChain, IEventNode)[Events.Count];

            var initialNodeIds =
                Zones.Select(x => x.EventNumber)
                .Concat(Npcs.Select(x => x.EventNumber))
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .OrderBy(x => x);

            foreach (var nodeId in initialNodeIds)
            {
                var chain = new EventChain(Chains.Count);
                chain.Events.Add(Events[nodeId]);
                Chains.Add(chain);
                ChainsByEventId[nodeId] = (chain, Events[nodeId]);
            }

            var nodesToCheck = new Queue<IEventNode>();
            foreach (var chain in Chains)
            {
                nodesToCheck.Enqueue(chain.Events[0]);
                bool headNode = true;
                while (nodesToCheck.Count > 0)
                {
                    var node = nodesToCheck.Dequeue();

                    if (!headNode)
                    {
                        if (ChainsByEventId[node.Id] != (null, null))
                            continue;

                        ChainsByEventId[node.Id] = (chain, node);
                        if (node != chain.Events[0])
                            chain.Events.Add(node);
                    }

                    if (node.NextEvent != null)
                        nodesToCheck.Enqueue(node.NextEvent);

                    if(node is IBranchNode branch && branch.NextEventWhenFalse != null)
                        nodesToCheck.Enqueue(branch.NextEventWhenFalse);

                    headNode = false;
                }
            }

            foreach (var chain in Chains)
            {
                var ordered = chain.Events.OrderBy(x => x.Id).ToList();
                ApiUtil.Assert(ordered[0] == chain.Events[0]);
                chain.Events.Clear();
                foreach(var e in ordered)
                    chain.Events.Add(e);
            }
        } //*/

        protected void BuildEventChains()
        {
            ChainsByEventId = new (EventChain, IEventNode)[Events.Count];
            var nodesToCheck = new Queue<IEventNode>();
            for (int i = 0; i < Events.Count; i++)
            {
                if (ChainsByEventId[i] != (null, null))
                    continue;

                var chain = new EventChain(Chains.Count);
                Chains.Add(chain);
                nodesToCheck.Enqueue(Events[i]);

                while (nodesToCheck.Count > 0)
                {
                    var node = nodesToCheck.Dequeue();
                    if (ChainsByEventId[node.Id] != (null, null))
                        continue;

                    ChainsByEventId[node.Id] = (chain, node);
                    chain.Events.Add(node);

                    if (node.NextEvent != null)
                        nodesToCheck.Enqueue(node.NextEvent);

                    if(node is IBranchNode branch && branch.NextEventWhenFalse != null)
                        nodesToCheck.Enqueue(branch.NextEventWhenFalse);
                }
            }

            foreach (var chain in Chains)
            {
                var ordered = chain.Events.OrderBy(x => x.Id).ToList();
                ApiUtil.Assert(ordered[0] == chain.Events[0]);
                chain.Events.Clear();
                foreach(var e in ordered)
                    chain.Events.Add(e);
            }

            var disableChainEvents = ChainsByEventId
                .Where(x => 
                    x.Item2.Event is DisableEventChainEvent
                    || (x.Item2.Event is ChangeIconEvent ci && ci.ChangeType == ChangeIconEvent.IconChangeType.Chain)
                ).ToList();
            Console.WriteLine(disableChainEvents.Count);
        } //*/

        protected void SerdesZones(ISerializer s)
        {
            int zoneCount = s.UInt16("ZoneCount", (ushort)Zones.Count(x => x.Global));
            s.List(Zones, zoneCount, (i, x, serializer) => MapEventZone.Serdes(x, serializer, 0xff));
            s.Check();

            int zoneOffset = zoneCount;
            for (byte y = 0; y < Height; y++)
            {
                zoneCount = s.UInt16("RowZones", (ushort)Zones.Count(x => x.Y == y && !x.Global));
                s.List(Zones, zoneCount, zoneOffset, (i, x, s) => MapEventZone.Serdes(x, s, y));
                zoneOffset += zoneCount;
            }
        }

        protected void SerdesEvents(ISerializer s)
        {
            ushort eventCount = s.UInt16("EventCount", (ushort)Events.Count);
            s.List(Events, eventCount, EventNode.Serdes);
            s.Check();
        }

        protected void Unswizzle()
        {
            // Resolve event indices to pointers
            foreach (var mapEvent in Events)
            {
                if (mapEvent.NextEventId.HasValue)
                    mapEvent.NextEvent = Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue)
                    q.NextEventWhenFalse = Events[q.NextEventWhenFalseId.Value];
            }

            BuildEventChains();

            foreach (var npc in Npcs)
                if (npc.Id != 0 && npc.EventNumber.HasValue)
                    (npc.Chain, npc.Node) = ChainsByEventId[npc.EventNumber.Value];

            foreach (var zone in Zones)
            {
                if (zone.EventNumber == null)
                    continue;

                var node = Events[zone.EventNumber.Value];
                var chain = ChainsByEventId[zone.EventNumber.Value].Item1;
                ApiUtil.Assert(chain.Events.First() == node);
                zone.Node = node;
                zone.Chain = chain;
            }

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
                if (zone.EventNumber.HasValue)
                    AddEventReference(zone.EventNumber.Value, zone);

            foreach (var npc in Npcs)
                if (npc.EventNumber.HasValue)
                    AddEventReference(npc.EventNumber.Value, npc);

            foreach (var e in Events)
            {
                if (e.NextEventId.HasValue)
                    AddEventReference(e.NextEventId.Value, e);

                if (e is BranchNode branch && branch.NextEventWhenFalseId.HasValue)
                    AddEventReference(branch.NextEventWhenFalseId.Value, e);
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
            foreach (var npc in Npcs)
                if (npc.Id != 0)
                    npc.LoadWaypoints(s);
        }
    }
}