using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using static System.FormattableString;

namespace UAlbion.Formats.MapEvents
{
    [DebuggerDisplay("{ToString()}")]
    public class EventNode : IEventNode
    {
        bool DirectSequence => (Next?.Id ?? Id + 1) == Id + 1;
        public override string ToString() => ToString(0);
        public virtual string ToString(int idOffset)
        {
            int id = Id - idOffset;
            int? next = Next?.Id - idOffset;
            return Invariant($"{(DirectSequence ? " " : "#")}{id}=>{next?.ToString(CultureInfo.InvariantCulture) ?? "!"}: {Event}");
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

        public static EventNode Serdes(ushort id, EventNode node, ISerializer s, AssetId chainSource, TextId textAssetId, AssetMapping mapping)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var initialPosition = s.Offset;
            var mapEvent = node?.Event as MapEvent;
            var @event = MapEvent.Serdes(mapEvent, s, chainSource, textAssetId, mapping);

            if (@event is IBranchingEvent be)
            {
                node ??= new BranchNode(id, be);
                var branch = (BranchNode)node;
                ushort? falseEventId = s.Transform<ushort, ushort?>(
                    nameof(branch.NextIfFalse),
                    branch.NextIfFalse?.Id,
                    S.UInt16,
                    MaxToNullConverter.Instance);

                if (falseEventId != null && branch.NextIfFalse == null)
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

        public static EventNode Parse(string s)
        {
            if (s == null || s.Length < 8)
                throw new FormatException($"Could not parse \"{s}\" as an event node: too short");

            int i = 0, n = 0, step = 0, id = -1, next = -1, nextIfFalse = -1;
            // Steps: 0=ID, 1=NextId (simple) 2=NextId (branch) 3=FalseId (branch) 4=Done
            if (s[i] == ' ' || s[i] == '#' || s[i] == '!') i++;
            for (; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case ' ':
                        if (step >= 4) { i++; goto done; }
                        throw new FormatException($"Unexpected '{s[i]}' while parsing \"{s}\" as an event node");
                    case '=':
                        if (step > 0) throw new FormatException($"Unexpected '{s[i]}' while parsing \"{s}\" as an event node");
                        i++;
                        if (s[i] != '>') throw new FormatException($"Unexpected '{s[i]}' while parsing \"{s}\" as an event node (expected '>')");
                        step = 1; id = n; n = 0;
                        break;
                    case '!':
                        if (n != 0) throw new FormatException($"Unexpected '{s[i]}' while parsing \"{s}\" as an event node");
                        n = -1;
                        break;
                    case '?': step = 2; id = n; n = 0; break;
                    case ':':
                        if (step == 1) { step = 4; next = n; }
                        else if (step == 2) { step = 3; next = n; n = 0; }
                        else if (step == 3) { step = 5; nextIfFalse = n; }
                        break;
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9': n *= 10; n += s[i] - '0'; break;
                    default: throw new FormatException($"Unexpected '{s[i]}' while parsing \"{s}\" as an event node");
                }
            }

            throw new FormatException($"Unexpected end of string while parsing \"{s}\" as an event node");

            done:
            if (id < 0) throw new FormatException($"Error parsing node id of event node \"{s}\"");
            var e = Api.Event.Parse(s.Substring(i));

            if (step == 5) // Branch node
            {
                if (!(e is IBranchingEvent be))
                    throw new FormatException($"Error parsing branch node \"{s}\": event \"{e}\" is not an IBranchingEvent");

                return new BranchNode((ushort)id, be)
                {
                    Next = next == -1 ? null : new DummyEventNode((ushort)next),
                    NextIfFalse = nextIfFalse == -1 ? null :new DummyEventNode((ushort)nextIfFalse)
                };
            }

            return new EventNode((ushort)id, e)
            {
                Next = next == -1 ? null : new DummyEventNode((ushort)next)
            };

            //  "{(DirectSequence ? " " : "#")}{id}=>{next?.ToString(CultureInfo.InvariantCulture) ?? "!"}: {Event}");
            // 00001
            //  1=>!: foo
            // #1=>!: foo
            //  1=>2: foo
            // #1=>2: foo
            // 0001122
            //  1?2:3: foo
            // #1?2:3: foo
            //  1?!:!: foo
            // #1?!:!: foo
            //  1?2:!: foo
            // #1?2:!: foo
            //  1?!:3: foo
            // #1?!:3: foo
        }
    }
}

