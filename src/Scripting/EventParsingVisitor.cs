using System;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class EventParsingVisitor : BaseAstBuilderVisitor
{
    int _nextEventId;
    protected override ICfgNode Build(Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);

        var builder = new UnformattedScriptBuilder(false);
        var formatter = new FormatScriptVisitor(builder);
        statement.Head.Accept(formatter);
        foreach (var part in statement.Parameters)
        {
            builder.Append(' ');
            part.Accept(formatter);
        }

        var formatted = builder.Build();
        var e = Event.Parse(formatted, out var error);
        if (e == null)
            throw new InvalidOperationException($"Could not parse \"{formatted}\" as an event: {error}");

        return Emit.Event(e, _nextEventId++);
    }

    protected override ICfgNode Build(Name name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var e = Event.Parse(name.Value, out var error);
        if (e == null)
            throw new InvalidOperationException($"Could not parse \"{name.Value}\" as an event: {error}");

        return Emit.Event(e, _nextEventId++);
    }
}