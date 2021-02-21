using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class EventSet
    {
        public EventSetId Id { get; private set; }
        EventChain[] _chains;
        EventNode[] _events;

        [JsonIgnore] public IList<EventChain> Chains => _chains;

#pragma warning disable CA2227 // Collection properties should be read only (setter is invoked via reflection by JSON deserialisation)
        public IList<ushort> ChainIds
        {
            get
            {
                if (_chains == null) 
                    return new List<ushort>();

                // First ensure event ids are up to date
                for (ushort i = 0; i < _events.Length; i++)
                    _events[i].Id = i;

                return _chains.Select(x => x.FirstEvent.Id).ToList();
            }
            set
            {
                _chains = new EventChain[value?.Count ?? 0];
                if (value == null)
                    return;

                for (int i = 0; i < _chains.Length; i++)
                {
                    var chain = new EventChain(i);
                    int lastEvent = i == value.Count - 1 ? _events.Length : value[i + 1];
                    for (int j = value[i]; j < lastEvent; j++)
                        chain.Events.Add(_events[j]);
                    _chains[i] = chain;
                }
            }
        }
#pragma warning restore CA2227 // Collection properties should be read only

        public IEnumerable<IEventNode> Events => _events;

        public static EventSet Serdes(EventSetId id, EventSet set, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            set ??= new EventSet { Id = id };
            var chainStarts = set.ChainIds;
            ushort chainCount = s.UInt16("ChainCount", (ushort)chainStarts.Count);
            ushort eventCount = s.UInt16("TotalEventCount", (ushort)(set._events?.Length ?? 0));

            set._events ??= new EventNode[eventCount];

            for (int i = 0; i < chainCount; i++)
            {
                if (i >= chainStarts.Count)
                    chainStarts.Add(0);
                chainStarts[i] = s.UInt16(null, (ushort)chainStarts[i]);
            }

            for (ushort i = 0; i < set._events.Length; i++)
                set._events[i] = EventNode.Serdes(i, set._events[i], s, id.ToEventText(), mapping);

            foreach (var e in set._events)
                e.Unswizzle(set._events);

            if (s.IsReading())
                set.ChainIds = chainStarts;

            return set;
        }
    }
}
