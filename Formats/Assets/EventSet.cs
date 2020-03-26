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
        ushort[] _chainStarts;
        EventNode[] _events;

        EventSet(EventSetId id) { Id = id; }

        public IEventNode GetChain(int chainId)
        {
            if (chainId >= _chainStarts.Length) return null;
            var firstEvent = _chainStarts[chainId];
            if (firstEvent >= _events.Length)
                return null;
            return _events[firstEvent];
        }

        public IEnumerable<IEventNode> Chains
        {
            get
            {
                for (int i = 0; i < _chainStarts.Length; i++)
                {
                    var chain = GetChain(i);
                    if (chain != null)
                        yield return chain;
                }
            }
        }

        public IEnumerable<IEventNode> Events => _events;

        public static EventSet Serdes(EventSetId eventSetId, EventSet set, ISerializer s)
        {
            set ??= new EventSet(eventSetId);
            ApiUtil.Assert(eventSetId == set.Id);
            ushort chainCount = s.UInt16("ChainCount", (ushort)(set._chainStarts?.Length ?? 0));
            ushort eventCount = s.UInt16("TotalEventCount", (ushort)(set._events?.Length ?? 0));

            if (set._chainStarts == null) set._chainStarts = new ushort[chainCount];
            if (set._events == null) set._events = new EventNode[eventCount];

            for (int i = 0; i < set._chainStarts.Length; i++)
                set._chainStarts[i] = s.UInt16(null, set._chainStarts[i]);

            var source = TextSource.EventSet(eventSetId);
            for (int i = 0; i < set._events.Length; i++)
                set._events[i] = EventNode.Serdes(i, set._events[i], s, source);

            foreach (var e in set._events)
            {
                ushort? nextId = e.NextEventId;
                e.NextEvent = nextId.HasValue ? set._events[nextId.Value] : null;

                if (e is BranchNode bn)
                {
                    nextId = bn.NextEventId;
                    // TODO: Warn if index invalid
                    bn.NextEvent = nextId >= eventCount ? null : nextId.HasValue ? set._events[nextId.Value] : null;
                    nextId = bn.NextEventWhenFalseId;
                    bn.NextEventWhenFalse = nextId >= eventCount ? null : nextId.HasValue ? set._events[nextId.Value] : null;
                }
            }

            return set;
        }
    }
}
