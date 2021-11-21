using System;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class EventSet
    {
        public EventSetId Id { get; private set; }
        [JsonInclude] public ushort[] Chains { get; private set; }
        [JsonIgnore] public EventNode[] Events { get; private set; }

        public string[] EventStrings // Used for JSON
        {
            get => Events?.Select(x => x.ToString()).ToArray();
            set
            {
                Events = value?.Select(EventNode.Parse).ToArray() ?? Array.Empty<EventNode>();
                foreach (var e in Events)
                    e.Unswizzle(Events);
            }
        }

        public static EventSet Serdes(EventSetId id, EventSet set, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            set ??= new EventSet { Id = id };
            var chains = set.Chains;
            ushort chainCount = s.UInt16("ChainCount", (ushort)(chains?.Length ?? 0));
            ushort eventCount = s.UInt16("TotalEventCount", (ushort)(set.Events?.Length ?? 0));

            chains ??= new ushort[chainCount];
            set.Events ??= new EventNode[eventCount];

            for (int i = 0; i < chainCount; i++)
                chains[i] = s.UInt16(null, chains[i]);

            set.Chains = chains;

            for (ushort i = 0; i < set.Events.Length; i++)
                set.Events[i] = MapEvent.SerdesNode(i, set.Events[i], s, id, id.ToEventText(), mapping);

            foreach (var e in set.Events)
                e.Unswizzle(set.Events);

            return set;
        }
    }
}
