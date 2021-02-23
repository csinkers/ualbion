using System;
using System.Linq;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class EventSet
    {
        EventNode[] _events;

        public EventSetId Id { get; private set; }
        public ushort[] Chains { get; private set; }

        [JsonIgnore] public EventNode[] Events => _events;

        public string[] EventStrings // Used for JSON
        {
            get => _events?.Select(x => x.ToString()).ToArray();
            set
            {
                _events = value?.Select(EventNode.Parse).ToArray() ?? Array.Empty<EventNode>();
                foreach (var e in _events)
                    e.Unswizzle(_events);
            }
        }

        public static EventSet Serdes(EventSetId id, EventSet set, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            set ??= new EventSet { Id = id };
            var chains = set.Chains;
            ushort chainCount = s.UInt16("ChainCount", (ushort)(chains?.Length ?? 0));
            ushort eventCount = s.UInt16("TotalEventCount", (ushort)(set._events?.Length ?? 0));

            chains ??= new ushort[chainCount];
            set._events ??= new EventNode[eventCount];

            for (int i = 0; i < chainCount; i++)
                chains[i] = s.UInt16(null, chains[i]);

            set.Chains = chains;

            for (ushort i = 0; i < set._events.Length; i++)
                set._events[i] = EventNode.Serdes(i, set._events[i], s, id, id.ToEventText(), mapping);

            foreach (var e in set._events)
                e.Unswizzle(set._events);

            return set;
        }
    }
}
