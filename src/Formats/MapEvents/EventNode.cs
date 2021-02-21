using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [DebuggerDisplay("{ToString()}")]
    [JsonConverter(typeof(ToStringJsonConverter))]
    public class EventNode : IEventNode
    {
        bool DirectSequence => (Next?.Id ?? Id + 1) == Id + 1;
        public override string ToString() => ToString(0);
        public virtual string ToString(int idOffset)
        {
            int id = Id - idOffset;
            int? next = Next?.Id - idOffset;
            return $"{(DirectSequence ? " " : "#")}{id}=>{next?.ToString(CultureInfo.InvariantCulture) ?? "!"}: {Event}";
        }

        public ushort Id { get; set; }
        public IEvent Event { get; }
        public IEventNode Next { get; set; }
        public EventNode(ushort id, IEvent @event)
        {
            Id = id;
            Event = @event;
        }

        public virtual void Unswizzle(IList<EventNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (!(Next is DummyEventNode dummy)) 
                return;

            if (dummy.Id >= nodes.Count)
            {
                ApiUtil.Assert($"Invalid event id: {Id} links to {dummy.Id}, but the set only contains {nodes.Count} events");
                Next = null;
            }
            else Next = nodes[dummy.Id];
        }

        public static EventNode Serdes(ushort id, EventNode node, ISerializer s, TextId textAssetId, AssetMapping mapping)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var initialPosition = s.Offset;
            var mapEvent = node?.Event as MapEvent;
            var @event = MapEvent.Serdes(mapEvent, s, textAssetId, mapping);

            if (@event is IBranchingEvent)
            {
                node ??= new BranchNode(id, @event);
                var branch = (BranchNode)node;
                ushort? falseEventId = s.Transform<ushort, ushort?>(
                    nameof(branch.NextIfFalse),
                    branch.NextIfFalse?.Id,
                    S.UInt16,
                    MaxToNullConverter.Instance);

                if(falseEventId != null && branch.NextIfFalse == null)
                    branch.NextIfFalse = new DummyEventNode(falseEventId.Value);
            }
            else
                node ??= new EventNode(id, @event);

            ushort? nextEventId = s.Transform<ushort, ushort?>(nameof(node.Next), node.Next?.Id, S.UInt16, MaxToNullConverter.Instance);
            if (nextEventId != null && node.Next == null)
                node.Next = new DummyEventNode(nextEventId.Value);

            long expectedPosition = initialPosition + 12;
            long actualPosition = s.Offset;
            ApiUtil.Assert(expectedPosition == actualPosition,
                $"Expected to have read {expectedPosition - initialPosition} bytes, but {actualPosition - initialPosition} have been read.");

            return node;
        }
    }
}

