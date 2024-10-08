﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UAlbion.Api.Eventing;

[DebuggerDisplay("{ToString()}")]
public class EventNode : IEventNode, IEquatable<EventNode>
{
    public const ushort UnusedEventId = 0xffff;
    bool DirectSequence => (Next?.Id ?? Id + 1) == Id + 1;
    public override string ToString()
    {
        var builder = new UnformattedScriptBuilder(false);
        Format(builder, 0);
        return builder.Build();
    }

    public virtual void Format(IScriptBuilder builder, int idOffset)
    {
        ArgumentNullException.ThrowIfNull(builder);
        int id = Id - idOffset;
        int? next = Next?.Id - idOffset;

        builder.Append(DirectSequence ? " " : "#");
        builder.Append(id);
        builder.Append("=>");
        builder.Append(next?.ToString() ?? "!");
        builder.Append(": ");
        Event.Format(builder);
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
        ArgumentNullException.ThrowIfNull(nodes);
        if (Next is not DummyEventNode dummy) 
            return;

        if (dummy.Id >= nodes.Count)
        {
            ApiUtil.Assert($"Invalid event id: {Id} links to {dummy.Id}, but the set only contains {nodes.Count} events");
            Next = null;
        }
        else Next = nodes[dummy.Id];
    }

    public static IList<EventNode> ParseRawEvents(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
            return null;
        var lines = ApiUtil.SplitLines(script);
        var events = lines.Select(Parse).ToList();
        foreach (var e in events)
            e.Unswizzle(events);
        return events;
    }

#pragma warning disable CA1502
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
        var e = Eventing.Event.Parse(s[i..], out var error);
        if (e == null)
            throw new FormatException($"Could not parse \"{s[i..]}\" as an event: {error}");

        if (step == 5) // Branch node
        {
            if (e is not IBranchingEvent be)
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

        //  "{(DirectSequence ? " " : "#")}{id}=>{next?.ToString() ?? "!"}: {Event}");
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
#pragma warning restore CA1502

    public bool Equals(EventNode other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && 
               Equals(Event.ToString(), other.Event.ToString()) && 
               Equals(Next?.Id, other.Next?.Id);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((EventNode)obj);
    }

    // ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => HashCode.Combine(Id, Event.GetHashCode(), Next?.Id);
    // ReSharper restore NonReadonlyMemberInGetHashCode
}
