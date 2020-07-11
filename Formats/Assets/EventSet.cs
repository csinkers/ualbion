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
        public EventChain[] Chains { get; private set; }
        EventNode[] _events;

        EventSet(EventSetId id) { Id = id; }

        public IEnumerable<IEventNode> Events => _events;

        public static EventSet Serdes(EventSetId eventSetId, EventSet set, ISerializer s)
        {
            set ??= new EventSet(eventSetId);
            s.Begin();
            var chainStarts = new List<int>();
            if (set.Chains != null)
            {
                for (int i = 0, j = 0; i < set.Chains.Length && j < set._events.Length; j++)
                {
                    if (set._events[j] == set.Chains[i].Events[0])
                    {
                        chainStarts.Add(j);
                        j++;
                    }
                }
            }

            ApiUtil.Assert(eventSetId == set.Id);
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
                set._events[i] = EventNode.Serdes(i, set._events[i], s, true, (ushort)eventSetId);

            foreach (var e in set._events)
                e.Unswizzle(set._events);

            if (set.Chains == null)
            {
                set.Chains = new EventChain[chainStarts.Count];
                for (int i = 0; i < set.Chains.Length; i++)
                {
                    var chain = new EventChain(i);
                    int lastEvent = i == chainStarts.Count - 1 ? set._events.Length : chainStarts[i + 1];
                    for (int j = chainStarts[i]; j < lastEvent; j++)
                        chain.Events.Add(set._events[j]);
                    set.Chains[i] = chain;
                }
            }

            s.End();
            return set;
        }
    }
}
