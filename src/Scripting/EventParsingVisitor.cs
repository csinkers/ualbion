using System;
using UAlbion.Api;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public class EventParsingVisitor : BaseBuilderAstVisitor
    {
        protected override ICfgNode Build(Statement s)
        {
            var formatter = new FormatScriptVisitor();
            s.Accept(formatter);
            var e = Event.Parse(formatter.Code);
            if (e == null)
                throw new InvalidOperationException($"Could not parse \"{formatter.Code}\" as an event");

            return Emit.Event(e);
        }

        protected override ICfgNode Build(Name name)
        {
            var e = Event.Parse(name.Value);
            return Emit.Event(e);
        }
    }
}