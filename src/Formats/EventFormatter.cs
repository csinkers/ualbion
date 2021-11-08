using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Scripting;

namespace UAlbion.Formats
{
    public class EventFormatter
    {
        readonly Func<StringId, string> _stringLoadFunc;
        readonly AssetId _textSourceId;

        public EventFormatter(Func<StringId, string> stringLoadFunc, AssetId textSourceId)
        {
            _stringLoadFunc = stringLoadFunc;
            _textSourceId = textSourceId;
        }

        public string Format(IEventNode e, int idOffset = 0)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var nodeText = e.ToString(idOffset);
            if (e.Event is TextEvent textEvent)
            {
                var text = _stringLoadFunc(new StringId(_textSourceId, textEvent.SubId));
                return $"{nodeText} // \"{text}\"";
            }

            return nodeText;
        }

        public void FormatChainDecompiled(StringBuilder sb, IEventNode firstEvent, IList<IEventNode> additionalEntryPoints, int indent)
        {
            var events = ExploreGraph(firstEvent);
            events.Remove(firstEvent);
            var sorted = new List<IEventNode> { firstEvent };
            sorted.AddRange(events);
            List<(string, ControlFlowGraph)> steps = new();
            try
            {
                var tree = Decompiler.Decompile(sorted, steps);
                var visitor = new EmitPseudocodeVisitor(sb) { IndentLevel = indent };
                tree.Accept(visitor);
            }
            catch (ControlFlowGraphException)
            {
                FormatChain(sb, firstEvent, indent); // Fallback to raw view
            }
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

        public string FormatChain(IEventNode firstEvent, int indent = 0)
        {
            var sb = new StringBuilder();
            FormatChain(sb, firstEvent, indent);
            return sb.ToString();
        }

        public void FormatChain(StringBuilder sb, IEventNode firstEvent, int indent = 0)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (firstEvent == null) return;

            var uniqueEvents = ExploreGraph(firstEvent);
            var sorted = uniqueEvents.OrderBy(x => x.Id).ToList();
            foreach (var e in sorted)
            {
                for (int i = 0; i < indent; i++)
                    sb.Append("    ");
                sb.AppendLine(Format(e, sorted[0].Id));
            }
        }
    }
}
