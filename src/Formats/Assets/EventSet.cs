using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class EventSet
    {
        public EventSetId Id { get; }
        EventChain[] _chains;
        EventNode[] _events;

        EventSet(EventSetId id) { Id = id; }

        public IList<EventChain> Chains => _chains;
        public IEnumerable<IEventNode> Events => _events;

        public static EventSet Serdes(int id, EventSet set, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            set ??= new EventSet((EventSetId)id);
            var chainStarts = new List<int>();
            if (set._chains != null)
            {
                for (int i = 0, j = 0; i < set._chains.Length && j < set._events.Length; j++)
                {
                    if (set._events[j] == set._chains[i].Events[0])
                    {
                        chainStarts.Add(j);
                        j++;
                    }
                }
            }

            ApiUtil.Assert((EventSetId)id == set.Id);
            ushort chainCount = s.UInt16("ChainCount", (ushort)chainStarts.Count);
            ushort eventCount = s.UInt16("TotalEventCount", (ushort)(set._events?.Length ?? 0));

            if (set._events == null) set._events = new EventNode[eventCount];

            for (int i = 0; i < chainCount; i++)
            {
                if (i >= chainStarts.Count)
                    chainStarts.Add(0);
                chainStarts[i] = s.UInt16(null, (ushort)chainStarts[i]);
            }

            for (ushort i = 0; i < set._events.Length; i++)
                set._events[i] = EventNode.Serdes(i, set._events[i], s, true, (ushort)id);

            foreach (var e in set._events)
                e.Unswizzle(set._events);

            if (set._chains == null)
            {
                set._chains = new EventChain[chainStarts.Count];
                for (int i = 0; i < set._chains.Length; i++)
                {
                    var chain = new EventChain(i);
                    int lastEvent = i == chainStarts.Count - 1 ? set._events.Length : chainStarts[i + 1];
                    for (int j = chainStarts[i]; j < lastEvent; j++)
                        chain.Events.Add(set._events[j]);
                    set._chains[i] = chain;
                }
            }

            return set;
        }
    }
}
