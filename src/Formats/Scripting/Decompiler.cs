using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UAlbion.Formats.MapEvents;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Formats.Scripting
{
    public static class Decompiler
    {
        public delegate ControlFlowGraph RecordFunc(string description, ControlFlowGraph graph);
        public static ICfgNode Decompile(List<IEventNode> nodes, List<(string, ControlFlowGraph)> steps = null)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Count == 0)
                throw new ArgumentException("Must supply at least one event node", nameof(nodes));

            RecordFunc record;
            if (steps != null)
            {
                var steps2 = steps;
                record = (description, graph) =>
                {
                    if (steps2.Count == 0 || steps[^1].Item2 != graph)
                        steps2.Add((description, graph));
                    return graph;
                };
            }
            else record = (_, x) => x;

            var rawGraph = record("Make blocks", MakeBlocks(nodes));
            var basicBlockGraph = record("Detect blocks", DetectBasicBlocks(rawGraph));
            return SimplifyGraph(basicBlockGraph, record).Head;
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
                nodes.Add(new Block(e.Event));
                if (mapping.ContainsKey(e.Id))
                    throw new InvalidOperationException($"Multiple events have the same id ({e.Id})!");
                mapping[e.Id] = i++;
            }
            nodes.Add(new EmptyNode());

            var edges = new List<(int, int, bool)> { (0, 1, true) };
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

        public static ControlFlowGraph SimplifyGraph(ControlFlowGraph graph, RecordFunc record)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            Func<string> vis = () => graph.Visualize();
            ControlFlowGraph previous = null;
            while (graph != previous)
            {
                previous = graph;
                graph = record("Reduce simple while", ReduceSimpleWhile(graph)); if (graph != previous) continue;
                graph = record("Reduce sequence", ReduceSequences(graph));       if (graph != previous) continue;
                graph = record("Reduce if-then", ReduceIfThen(graph));           if (graph != previous) continue;
                graph = record("Reduce if-then-else", ReduceIfThenElse(graph));  if (graph != previous) continue;
                graph = record("Reduce loop parts", ReduceLoopParts(graph));     if (graph != previous) continue;
                graph = record("Reduce simple loop", ReduceSimpleLoops(graph));  if (graph != previous) continue;
                graph = record("Reduce SESE region", ReduceSeseRegions(graph));  if (graph != previous) continue;
                graph = record("Defragment", graph.Defragment());
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

                region.Contents.Nodes[path[i]].ToPseudocode(sb, false, false);
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
                #if DEBUG
                Func<string> vis = () => graph.ToVis().AddPointer("index", index).ToString();
                #endif
                if (graph.Nodes[index] is not ICondition condition)
                    throw new ControlFlowGraphException($"While-loop header {index} was not a condition", graph);

                var updated = graph
                    .ReplaceNode(index, new WhileLoop(condition, null))
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
                #if DEBUG
                Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("child", child).ToString();
                #endif

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
                #if DEBUG
                Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("after", after).AddPointer("then", then).ToString();
                #endif

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

                if (graph.Nodes[head] is not ICondition condition)
                    throw new ControlFlowGraphException($"If-then header {head} was not a condition", graph);

                var newNode = new IfThen(condition, graph.Nodes[then]);
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
                int after = -1;

                #if DEBUG
                Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("after", after).AddPointer("left", left).AddPointer("right", right).ToString();
                #endif

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
                after = isRegularIfThenElse ? leftChildren[0] : -1;

                if (after == head)
                    continue;

                if (graph.Nodes[head] is not ICondition condition)
                    throw new ControlFlowGraphException($"If-then-else header {head} was not a condition", graph);

                var newNode = new IfThenElse(
                    condition,
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
#if DEBUG
                        Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString();
#endif
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
#if DEBUG
            Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString();
#endif
            if (part.OutsideEntry || part.Tail || part.Header || !part.Continue || part.Break) 
                return graph;

            var children = graph.Children(index);
            var continueNode = new ContinueNode();
            if (children.Length == 1)
            {
                var seq = new Sequence(graph.Nodes[index], continueNode);
                return graph
                    .ReplaceNode(index, seq)
                    .RemoveEdge(index, loop.Header.Index);
            }

            if (children.Length == 2)
                return ReplaceLoopBranch(graph, index, loop.Header.Index, continueNode);

            throw new ControlFlowGraphException($"Continue at {index} has unexpected child count ({children.Length})", graph);
        }

        static ControlFlowGraph ReduceBreak(LoopPart part, ControlFlowGraph graph, int index, CfgLoop loop)
        {
#if DEBUG
            Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString();
#endif
            if (part.OutsideEntry || part.Tail || part.Header || part.Continue || !part.Break) 
                return graph;

            var children = graph.Children(index);

            var breakNode = new BreakNode();
            if (children.Length == 1)
            {
                var seq = new Sequence(graph.Nodes[index], breakNode);
                return graph
                    .ReplaceNode(index, seq)
                    .RemoveEdge(index, loop.Header.Index);
            }

            if (children.Length == 2)
            {
                bool isfirstChildInLoop = loop.Body.Any(x => x.Index == children[0]);
                var target = isfirstChildInLoop ? children[1] : children[0];

                if (target != loop.MainExit)
                {
                    var targetChildren = graph.Children(target);
                    if (targetChildren.Length != 1 || targetChildren[0] != loop.MainExit)
                        return graph;

                    bool label = graph.GetEdgeLabel(index, target);

                    if (graph.Nodes[index] is not ICondition condition)
                        throw new ControlFlowGraphException($"Branching continue {index} was not a condition", graph);

                    if (!label)
                        condition = new Negation(condition);

                    var newBlock = new Sequence(graph.Nodes[target], new BreakNode());
                    var ifNode = new IfThen(condition, newBlock);
                    return graph
                        .RemoveNode(target)
                        .ReplaceNode(index, ifNode);
                }

                return ReplaceLoopBranch(graph, index, target, breakNode);
            }

            throw new ControlFlowGraphException($"Break at {index} has unexpected child count ({children.Length})", graph);
        }

        static ControlFlowGraph ReplaceLoopBranch(ControlFlowGraph graph, int index, int target, ICfgNode newNode)
        {
            bool label = graph.GetEdgeLabel(index, target);
            if (graph.Nodes[index] is not ICondition condition)
                throw new ControlFlowGraphException($"Branching continue {index} was not a condition", graph);

            if (!label)
                condition = new Negation(condition);

            var ifNode = new IfThen(condition, newNode);
            return graph
                .ReplaceNode(index, ifNode)
                .RemoveEdge(index, target);
        }

        public static ControlFlowGraph ReduceSimpleLoops(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var loops = graph.GetLoops();
            foreach (var head in graph.GetDfsPostOrder())
            {
                foreach (CfgLoop loop in loops)
                {
                #if DEBUG
                Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("loop", loop.Header.Index).ToString();
                #endif

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
#if DEBUG
            Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("tail", tail).ToString();
#endif
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 1)
            {
                bool isfirstChildInLoop = head == children[0];
                var target = isfirstChildInLoop ? children[1] : children[0];
                return ReplaceLoopBranch(graph, tail, target, new BreakNode());
            }

            if (graph.Nodes[head] is not ICondition condition)
                throw new ControlFlowGraphException($"While-loop header {head} was not a condition", graph);

            var newNode = new WhileLoop(condition, graph.Nodes[tail]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        static ControlFlowGraph ReduceDoLoop(ControlFlowGraph graph, int head, int tail)
        {
#if DEBUG
            Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("tail", tail).ToString();
#endif
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 2)
                return updated;

            foreach (int child in children)
                if (child != head)
                    updated = updated.AddEdge(head, child, true);

            if (graph.Nodes[tail] is not ICondition condition)
                throw new ControlFlowGraphException($"Do-loop tail {tail} was not a condition", graph);

            var newNode = new DoLoop(condition, graph.Nodes[head]);
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
                #if DEBUG
                Func<string> vis = () =>
                {
                     var d = graph.ToVis(); 
                     foreach(var n in d.Nodes)
                        if (region.Contains(int.Parse(n.Id, CultureInfo.InvariantCulture)))
                            n.Color = "#4040b0";
                     return d.ToString();
                };
                #endif
                bool containsOther = regions.Any(x => x != region && !x.Except(region).Any());
                if (containsOther)
                    continue;

                var cut = graph.Cut(region);

                if (cut.Cut.IsCyclic())
                    continue;

                var cutEntries = cut.RemainderToCutEdges.Select(x => x.end).Distinct().ToList();
                var cutExits = cut.CutToRemainderEdges.Select(x => x.start).Distinct().ToList();

                if (cutEntries.Count > 1)
                {
                    var regionNodes = string.Join(", ", region.OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                    var entryNodes = string.Join(", ", cutEntries.OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                    throw new ControlFlowGraphException($"SESE cut ({regionNodes}) had multiple entry nodes: {entryNodes}", graph);
                }

                if (cutExits.Count > 1)
                {
                    var regionNodes = string.Join(", ", region.OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                    var exitNodes = string.Join(", ", cutExits.OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                    throw new ControlFlowGraphException($"SESE cut ({regionNodes}) had multiple exit nodes: {exitNodes}", graph);
                }

                if (cutEntries[0] != cut.Cut.HeadIndex)
                {
                    var regionNodes = string.Join(", ", region.OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                    throw new ControlFlowGraphException( $"SESE cut ({regionNodes}) had unique entry node {cut.Cut.HeadIndex} " +
                                                         $"but all incoming edges point at {cutEntries[0]}", graph);
                }

                var newNode = new SeseRegion(cut.Cut);
                var updated = cut.Remainder.AddNode(newNode, out var newNodeIndex);

                foreach (var (start, _, label) in cut.RemainderToCutEdges)
                    updated = updated.AddEdge(start, newNodeIndex, label);

                foreach (var (_, end, label) in cut.CutToRemainderEdges)
                    updated = updated.AddEdge(newNodeIndex, end, label);

                return updated.RemoveNodes(region);
            }

            return graph;
        }
    }
}
#pragma warning restore 8321