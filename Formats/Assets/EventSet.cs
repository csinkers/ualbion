using System.Collections.Generic;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class EventSet
    {
        ushort[] _chainStarts;
        IEventNode[] _events;

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

        public static void Translate(EventSet set, ISerializer s, string name, AssetInfo config)
        {
            ushort chainCount = (ushort)(set._chainStarts?.Length ?? 0);
            ushort eventCount = (ushort)(set._events?.Length ?? 0);
            s.UInt16("ChainCount", () => chainCount, x => chainCount = x);
            s.UInt16("TotalEventCount", () => eventCount, x => eventCount = x);

            if (set._chainStarts == null) set._chainStarts = new ushort[chainCount];
            if (set._events == null) set._events = new IEventNode[eventCount];

            for (int i = 0; i < set._chainStarts.Length; i++)
                s.UInt16("c" + i, () => set._chainStarts[i], x => set._chainStarts[i] = x);

            for (int i = 0; i < set._events.Length; i++)
            {
                s.Meta("e" + i,
                    x => set._events[i] = EventNode.Translate(null, x, i),
                    x => EventNode.Translate((EventNode)set._events[i], x, i)
                );
            }

            foreach (var e in set._events)
            {
                if (e is EventNode en)
                {
                    ushort? nextId = en.NextEventId;
                    en.NextEvent = nextId.HasValue ? set._events[nextId.Value] : null;
                }
                if (e is BranchNode bn)
                {
                    ushort? nextId = bn.NextEventId;
                    // TODO: Warn if index invalid
                    bn.NextEvent = nextId >= eventCount ? null : nextId.HasValue ? set._events[nextId.Value] : null;
                    nextId = bn.NextEventWhenFalseId;
                    bn.NextEventWhenFalse = nextId >= eventCount ? null : nextId.HasValue ? set._events[nextId.Value] : null;
                }
            }
            s.CheckEntireLengthRead();
        }
    }
}