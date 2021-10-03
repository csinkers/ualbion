using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Scripting
{
    public static class Decompiler
    {
        public static ICfgNode Decompile(List<IEventNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Count == 0)
                throw new ArgumentException("Must supply at least one event node", nameof(nodes));

            var rawGraph = MakeBlocks(nodes);
            var basicBlockGraph = DetectBasicBlocks(rawGraph);
            return SimplifyGraph(basicBlockGraph).Head;
        }

        public static ControlFlowGraph DetectBasicBlocks(ControlFlowGraph graph)
        {
            return graph;
        }

        public static ControlFlowGraph MakeBlocks(List<IEventNode> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            var nodes = new List<ICfgNode>();
            var mapping = new Dictionary<ushort, int>();
            int i = 1;
            nodes.Add(new EmptyNode());
            foreach (var e in events)
            {
                nodes.Add(new Block(new[] { e.Event }));
                if (mapping.ContainsKey(e.Id))
                    throw new InvalidOperationException($"Multiple events have the same id ({e.Id})!");
                mapping[e.Id] = i++;
            }
            nodes.Add(new EmptyNode());

            var edges = new List<(int, int, bool)>();
            foreach (var e in events)
            {
                var start = mapping[e.Id];
                var end = e.Next == null ? i : mapping[e.Next.Id];
                edges.Add((start, end, true));

                if (e is not IBranchNode branch) 
                    continue;

                end = branch.NextIfFalse == null ? i : mapping[branch.NextIfFalse.Id];
                edges.Add((start, end, false));
            }

            return new ControlFlowGraph(0, nodes, edges);
        }

        public static ControlFlowGraph SimplifyGraph(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            ControlFlowGraph previous = null;
            while (graph != previous)
            {
                previous = graph;
                graph = ReduceSimpleWhile(graph);
                graph = ReduceSequences(graph);
                graph = ReduceIfThen(graph);
                graph = ReduceIfThenElse(graph);
                graph = ReduceLoopParts(graph);
                graph = ReduceSimpleLoops(graph);
                if (previous == graph)
                    graph = ReduceSeseRegions(graph);
                graph = graph.Defragment();
            }
            return graph;
        }

        public static string ConditionPathToExpression(List<int> path, SeseRegion region)
        {
            // TODO: Convert to ICondition construction + handle SESE w/ reaching paths
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (region == null) throw new ArgumentNullException(nameof(region));
            var sb = new StringBuilder();
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (!region.DecisionNodes.Contains(path[i]) || !region.CodeNodes.Contains(path[i + 1])) 
                    continue;

                bool cond = false;
                foreach (var e in region.Contents.Edges)
                {
                    if (e.start == path[i] &&
                        e.end == path[i + 1])
                    {
                        cond = region.Contents.GetEdgeLabel(e.start, e.end);
                        break;
                    }
                }

                if (sb.Length != 0) sb.Append(" && ");
                if (!cond)
                    sb.Append('!');

                region.Contents.Nodes[path[i]].ToPseudocode(sb, "");
            }
            return sb.ToString();
        }

        public static ControlFlowGraph ReduceSimpleWhile(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var simpleLoopIndices =
                from index in graph.GetDfsPostOrder()
                let children = graph.Children(index)
                where children.Length == 2 && children.Contains(index)
                select index;

            foreach (var index in simpleLoopIndices)
            {
                var updated = graph
                    .ReplaceNode(index, new WhileLoop(graph.Nodes[index], null))
                    .RemoveEdge(index, index);

                return updated;
            }

            return graph;
        }

        public static ControlFlowGraph ReduceSequences(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            foreach (var index in graph.GetDfsPostOrder())
            {
                var children = graph.Children(index);
                if (children.Length != 1 || children[0] == index)
                    continue;

                int child = children[0];
                var childsParents = graph.Parents(child);
                var grandChildren = graph.Children(child);

                if (childsParents.Length != 1 || grandChildren.Length >= 2) 
                    continue; // Is a jump target from somewhere else as well - can't combine

                if (grandChildren.Length == 1 && (grandChildren[0] == index || grandChildren[0] == child))
                    continue; // Loops around, not a sequence

                var newNode = 
                    graph.Nodes[index] is Sequence existing
                    ? new Sequence(existing.Nodes, graph.Nodes[child])
                    : new Sequence(graph.Nodes[index], graph.Nodes[child]);

                var updated = graph
                    .RemoveNode(child)
                    .ReplaceNode(index, newNode);

                foreach (var grandChild in grandChildren)
                    updated = updated.AddEdge(index, grandChild, graph.GetEdgeLabel(child, grandChild));

                return updated;
            }

            return graph;
        }

        public static ControlFlowGraph ReduceIfThen(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var order = graph.GetDfsPostOrder();
            foreach (var head in order)
            {
                var children = graph.Children(head);
                if (children.Length != 2) 
                    continue;

                int after = -1;
                var then = -1;

                var parents0 = graph.Parents(children[0]);
                var parents1 = graph.Parents(children[1]);
                if (parents0.Length == 1)
                {
                    var grandChildren = graph.Children(children[0]);
                    if (grandChildren.Length == 1 && grandChildren[0] == children[1])
                    {
                        then = children[0];
                        after = children[1];
                    }
                }
                else if (parents1.Length == 1)
                {
                    var grandChildren = graph.Children(children[1]);
                    if (grandChildren.Length == 1 && grandChildren[0] == children[0])
                    {
                        then = children[1];
                        after = children[0];
                    }
                }

                if (after == -1 || then == -1 || after == head) 
                    continue;

                var newNode = new IfThen(graph.Nodes[head], graph.Nodes[then]);
                return graph.RemoveNode(then).ReplaceNode(head, newNode);
            }

            return graph;
        }

        public static ControlFlowGraph ReduceIfThenElse(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var order = graph.GetDfsPostOrder();
            foreach (var head in order)
            {
                var children = graph.Children(head);
                if (children.Length != 2) 
                    continue;

                var left = children[0];
                var right = children[1];
                var leftParents = graph.Parents(left);
                var rightParents = graph.Parents(right);
                var leftChildren = graph.Children(left);
                var rightChildren = graph.Children(right);

                bool isRegularIfThenElse =
                    leftParents.Length == 1 && rightParents.Length == 1 &&
                    leftChildren.Length == 1 && rightChildren.Length == 1 &&
                    leftChildren[0] == rightChildren[0];

                bool isTerminalIfThenElse = // TODO: Remove?
                    leftParents.Length == 1 && rightParents.Length == 1 &&
                    leftChildren.Length == 0 && rightChildren.Length == 0;

                if (!isRegularIfThenElse && !isTerminalIfThenElse)
                    continue;

                bool leftLabel = graph.GetEdgeLabel(head, left);
                var thenIndex = leftLabel ? left : right;
                var elseIndex = leftLabel ? right : left;
                var after = isRegularIfThenElse ? leftChildren[0] : -1;

                if (after == head)
                    continue;

                var newNode = new IfThenElse(
                    graph.Nodes[head],
                    graph.Nodes[thenIndex],
                    graph.Nodes[elseIndex]);

                var updated = graph;
                if (isRegularIfThenElse)
                    updated = updated.AddEdge(head, after, true);

                return updated
                    .RemoveNode(thenIndex)
                    .RemoveNode(elseIndex)
                    .ReplaceNode(head, newNode);
            }

            return graph;
        }

        public static ControlFlowGraph ReduceLoopParts(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var loops = graph.GetLoops();
            foreach (var index in graph.GetDfsPostOrder())
            {
                foreach (var loop in loops)
                {
                    if (loop.IsMultiExit)
                        continue;

                    foreach (var part in loop.Body)
                    {
                        if (part.Index != index)
                            continue;

                        var updated = ReduceContinue(part, graph, index, loop);
                        if (updated != graph)
                            return updated;

                        updated = ReduceBreak(part, graph, index, loop);
                        if (updated != graph)
                            return updated;
                    }
                }
            }
            return graph;
        }

        static ControlFlowGraph ReduceContinue(LoopPart part, ControlFlowGraph graph, int index, CfgLoop loop)
        {
            if (part.OutsideEntry || part.Tail || part.Header || !part.Continue || part.Break) 
                return graph;

            var children = graph.Children(index);
            if (children.Length != 2)
                return graph;

            var newNode = new ContinueNode(graph.Nodes[index]);
            return graph
                .ReplaceNode(index, newNode)
                .RemoveEdge(index, loop.Header.Index);
        }

        static ControlFlowGraph ReduceBreak(LoopPart part, ControlFlowGraph graph, int index, CfgLoop loop)
        {
            if (part.OutsideEntry || part.Tail || part.Header || part.Continue || !part.Break) 
                return graph;

            var children = graph.Children(index);
            if (children.Length != 2)
                return graph;

            bool isFirst = loop.Body.Any(x => x.Index == children[0]);
            var newNode = new BreakNode(graph.Nodes[index]);

            return graph
                .ReplaceNode(index, newNode)
                .RemoveEdge(index, isFirst ? children[1] : children[0]);
        }

        public static ControlFlowGraph ReduceSimpleLoops(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var loops = graph.GetLoops();
            foreach (var head in graph.GetDfsPostOrder())
            {
                foreach (CfgLoop loop in loops)
                {
                    if (loop.Header.Index != head
                     || loop.IsMultiExit
                     || loop.Body.Count != 1
                     || loop.Body[0].OutsideEntry)
                    {
                        continue;
                    }

                    int tail = loop.Body[0].Index;
                    if (loop.Header.Break)
                    {
                        var updated = ReduceWhileLoop(graph, head, tail);
                        if (updated != graph)
                            return updated;
                    }
                    else
                    {
                        var updated = ReduceDoLoop(graph, head, tail);
                        if (updated != graph)
                            return updated;
                    }

                }
            }
            return graph;
        }

        static ControlFlowGraph ReduceWhileLoop(ControlFlowGraph graph, int head, int tail)
        {
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 1)
            {
                var breakNode = new BreakNode(graph.Nodes[tail]);
                foreach (int child in children)
                    if (child != head)
                        updated = updated.RemoveEdge(tail, child);

                return updated.ReplaceNode(tail, breakNode);
            }

            var newNode = new WhileLoop(graph.Nodes[head], graph.Nodes[tail]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        static ControlFlowGraph ReduceDoLoop(ControlFlowGraph graph, int head, int tail)
        {
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 2)
                return updated;

            foreach (int child in children)
                if (child != head)
                    updated = updated.AddEdge(head, child, true);

            var newNode = new DoLoop(graph.Nodes[head], graph.Nodes[tail]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        public static ControlFlowGraph ReduceSeseRegions(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var regions = graph.GetAllSeseRegions();
            foreach (var region in regions)
            {
                bool containsOther = regions.Any(x => x != region && !x.Except(region).Any());
                if (containsOther)
                    continue;

                var gr = graph.GetCutOut(region);
                if (gr.IsCyclic()) continue;

                var newNode = new SeseRegion(gr);
                var updated = graph.ReplaceNode(region[0], newNode);

                int last = region[^1];
                if (graph.Children(last).Length > 0)
                    updated = updated.AddEdge(region[0], graph.Children(last)[0], true);

                region.RemoveAt(0);
                return updated.RemoveNodes(region);
            }

            return graph;
        }
    }
}