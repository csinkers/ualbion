using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Scripting;
using UAlbion.Scripting.Ast;

namespace UAlbion.Formats;

public class EventFormatter : IEventFormatter
{
    readonly Func<StringId, string> _stringLoadFunc;
    readonly AssetId _textSourceId;

    public EventFormatter(Func<StringId, string> stringLoadFunc, AssetId textSourceId)
    {
        _stringLoadFunc = stringLoadFunc;
        _textSourceId = textSourceId;
    }

    public void Format(IScriptBuilder builder, IEventNode e, int idOffset = 0)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (e == null) throw new ArgumentNullException(nameof(e));
        e.Format(builder, idOffset);

        if (e.Event is TextEvent textEvent && _stringLoadFunc != null)
        {
            var text = _stringLoadFunc(textEvent.ToId(_textSourceId)).Replace("\"", "\\\"");
            builder.Add(ScriptPartType.Comment, $" ; \"{text}\"");
        }
    }

    public void Format(IScriptBuilder builder, IEvent e)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (e == null) throw new ArgumentNullException(nameof(e));
        e.Format(builder);

        if (e is TextEvent textEvent && _stringLoadFunc != null)
        {
            var text = _stringLoadFunc(textEvent.ToId(_textSourceId)).Replace("\"", "\\\"");
            builder.Add(ScriptPartType.Comment, $" ; \"{text}\"");
        }
    }

    public DecompilationResult Decompile<T>(
        IList<T> events,
        IEnumerable<ushort> chains,
        IEnumerable<ushort> additionalEntryPoints,
        int indent = 0) where T : IEventNode
    {
        if (events.Count == 0)
            return new DecompilationResult();

        List<(string, IGraph)> steps = new();
        try
        {
            var trees = Decompiler.Decompile(events, chains, additionalEntryPoints, steps);
            return FormatGraphsAsBlocks(trees, indent);
        }
        catch (ControlFlowGraphException)
        {
            var builder = new DecompilationResultBuilder(false);
            FormatEventSet(builder, events, indent); // Fallback to raw view
            return builder.Build(Enumerable.Empty<ICfgNode>());
        }
    }

    public DecompilationResult FormatGraphsAsBlocks(IEnumerable<ICfgNode> trees, int indent)
    {
        if (trees == null) throw new ArgumentNullException(nameof(trees));
        var builder = new DecompilationResultBuilder(false);
        var counter = new CountEventsVisitor();

        bool first = true;
        bool lastWasTrivial = false;
        foreach (var tree in trees)
        {
            if (!first)
                builder.AppendLine();

            tree.Accept(counter);
            var eventCount = counter.Count;
            counter.Reset();

            if (eventCount > 1)
            {
                if (lastWasTrivial)
                    builder.AppendLine();
                builder.AppendLine("{");

                var visitor = new FormatScriptVisitor(builder) { PrettyPrint = true, IndentLevel = indent, Formatter = this };
                visitor.IndentLevel += visitor.TabSize;
                tree.Accept(visitor);

                builder.AppendLine();
                builder.AppendLine("}");
                lastWasTrivial = false;
            }
            else
            {
                builder.Append("{ ");
                var visitor = new FormatScriptVisitor(builder) { PrettyPrint = false };
                tree.Accept(visitor);
                builder.Append(" }");
                lastWasTrivial = true;
            }
            first = false;
        }

        builder.AppendLine();
        return builder.Build(trees);
    }

    static HashSet<IEventNode> ExploreGraph(IEventNode head)
    {
        var uniqueEvents = new HashSet<IEventNode>();
        void Visit(IEventNode e)
        {
            while (true)
            {
                if (e == null)
                    return;

                if (!uniqueEvents.Add(e))
                    break;

                if (e is IBranchNode branch)
                    Visit(branch.NextIfFalse);
                e = e.Next;
            }
        }

        Visit(head);
        return uniqueEvents;
    }

    public void FormatChain(IScriptBuilder builder, IEventNode firstEvent, int indent = 0)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (firstEvent == null) return;

        var uniqueEvents = ExploreGraph(firstEvent);
        var sorted = uniqueEvents.OrderBy(x => x.Id).ToList();
        foreach (var e in sorted)
        {
            builder.Append(new string(' ', 4 * indent));
            Format(builder, e, sorted[0].Id);
        }
    }

    public void FormatEventSet<T>(IScriptBuilder builder, IList<T> events, int indent = 0) where T : IEventNode
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (events == null) return;

        foreach (var e in events)
        {
            builder.Append(new string(' ', 4 * indent));
            Format(builder, e, events[0].Id);
        }
    }
}