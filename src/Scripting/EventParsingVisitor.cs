using System;
using System.Text;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class EventParsingVisitor : BaseAstBuilderVisitor
{
    int _nextEventId;
    protected override ICfgNode Build(Statement statement)
    {
        var sb = new StringBuilder();
        var formatter = new FormatScriptVisitor(sb, null);
        statement.Head.Accept(formatter);
        foreach (var part in statement.Parameters)
        {
            sb.Append(' ');
            part.Accept(formatter);
        }

        var e = Event.Parse(formatter.Code);
        if (e == null)
            throw new InvalidOperationException($"Could not parse \"{formatter.Code}\" as an event");

        return Emit.Event(e, _nextEventId++);
    }

    protected override ICfgNode Build(Name name)
    {
        var e = Event.Parse(name.Value);
        return Emit.Event(e, _nextEventId++);
    }
}