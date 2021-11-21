using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UAlbion.Api;
using UAlbion.Scripting.Ast;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Scripting
{
    public static class Decompiler
    {
        const string DummyLabelPrefix = "L_";

        public delegate ControlFlowGraph RecordFunc(string description, ControlFlowGraph graph);
        public static List<ICfgNode> Decompile<T>(
            IList<T> nodes,
            IEnumerable<ushort> chains,
            IEnumerable<ushort> additionalEntryPoints,
            List<(string, ControlFlowGraph)> steps = null) where T : IEventNode
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

            var graphs = BuildEventRegions(nodes, chains, additionalEntryPoints);
            var results = new List<ICfgNode>();
            for (var index = 0; index < graphs.Count; index++)
            {
                var graph = record($"Make region {index}", graphs[index]);
                results.Add(SimplifyGraph(graph, record).Head);
            }

            return results;
        }

        public static List<ControlFlowGraph> BuildEventRegions<T>(
            IList<T> events,
            IEnumerable<ushort> chains,
            IEnumerable<ushort> additionalEntryPoints) where T : IEventNode
        {
            int entry = events.Count;
            int exit = events.Count + 1;
            var results = new List<ControlFlowGraph>();
            var mapping = new Dictionary<int, int>();
            var queue = new Queue<IEventNode>();

            for (int i = 0; i < events.Count; i++)
                if (events[i].Id != i)
                    throw new ArgumentException($"Event {i} in the event list had id {events[i].Id}!");

            void Visit(int head, string label)
            {
                if (head == 0xffff) // Terminal value for unused chains etc
                    return;

                if (head > events.Count)
                    throw new ArgumentException($"Entry node {head} was given, but there are only {events.Count} nodes");

                if (mapping.TryGetValue(head, out var graphIndex))
                {
                    results[graphIndex] = results[graphIndex].InsertBefore(head, Emit.Label(label));
                    return;
                }

                queue.Enqueue(events[head]);

                var edges = new List<(int, int, bool)>();
                var nodes = new ICfgNode[events.Count + 2];
                nodes[entry] = Emit.Empty();
                nodes[exit] = Emit.Empty();

                while (queue.TryDequeue(out var node))
                {
                    int i = node.Id;
                    if (mapping.ContainsKey(i))
                        continue;

                    mapping[i] = results.Count;
                    nodes[i] = Emit.Event(node.Event);

                    if (node.Next != null)
                    {
                        queue.Enqueue(node.Next);
                        edges.Add((i, node.Next.Id, true));
                    }
                    else edges.Add((i, exit, true));

                    if (node is IBranchNode branch && branch.NextIfFalse != branch.Next)
                    {
                        if (branch.NextIfFalse != null)
                        {
                            queue.Enqueue(branch.NextIfFalse);
                            edges.Add((i, branch.NextIfFalse.Id, false));
                        }
                        else edges.Add((i, exit, false));
                    }
                }
                edges.Add((entry, head, true));

                var graph = new ControlFlowGraph(entry, nodes, edges);
                graph = graph.InsertBefore(head, Emit.Label(label));
                results.Add(graph);
            }

            if (chains != null)
            {
                int index = 0;
                foreach (var chain in chains)
                    Visit(chain, $"Chain{index++}");
            }

            if (additionalEntryPoints != null)
                foreach (var head in additionalEntryPoints)
                    Visit(head, $"Event{head}");

            for (var i = 0; i < results.Count; i++)
                results[i] = results[i].Defragment();

            return results;
        }

        public static ControlFlowGraph SimplifyGraph(ControlFlowGraph graph, RecordFunc record)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            Func<string> vis = () => graph.Visualize();
            ControlFlowGraph previous = null;
            while (graph != previous)
            {
                previous = graph;
                // graph = record("Defragment", graph.Defragment());
                graph = record("Reduce simple while", ReduceSimpleWhile(graph));  if (graph != previous) continue;
                graph = record("Reduce sequence", ReduceSequences(graph, false)); if (graph != previous) continue;
                graph = record("Reduce if-then", ReduceIfThen(graph));            if (graph != previous) continue;
                graph = record("Reduce if-then-else", ReduceIfThenElse(graph));   if (graph != previous) continue;
                graph = record("Reduce loop parts", ReduceLoopParts(graph));      if (graph != previous) continue;
                graph = record("Reduce simple loop", ReduceSimpleLoops(graph));   if (graph != previous) continue;
                graph = record("Reduce SESE region", ReduceSeseRegions(graph));   if (graph != previous) continue;
                graph = record("Reduce sequence", ReduceSequences(graph, true));
            }
            graph = record("Defragment", graph.Defragment());

            return CfgRelabeller.Relabel(graph, DummyLabelPrefix);
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

                var updated = graph
                    .ReplaceNode(index, Emit.While(graph.Nodes[index], null))
                    .RemoveEdge(index, index);

                return updated;
            }

            return graph;
        }

        public static ControlFlowGraph ReduceSequences(ControlFlowGraph graph, bool reduceEmptyNodes)
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

                if (childsParents.Length != 1 || grandChildren.Length > 1)
                    continue; // Is a jump target from somewhere else as well - can't combine

                if (grandChildren.Length == 1 && (grandChildren[0] == index || grandChildren[0] == child))
                    continue; // Loops around, not a sequence

                var node = graph.Nodes[index];
                var childNode = graph.Nodes[child];

                if (reduceEmptyNodes && (node is EmptyNode || childNode is EmptyNode))
                    continue; // Don't collapse the entry/exit nodes until the end

                var newNode = 
                    node is Sequence existing
                    ? Emit.Seq(existing.Statements.Append(graph.Nodes[child]).ToArray())
                    : Emit.Seq(node, childNode);

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

                var newNode = Emit.If(graph.Nodes[head], graph.Nodes[then]);
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

                var newNode = Emit.IfElse(
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
            var continueNode = Emit.Continue();
            if (children.Length == 1)
            {
                var seq = Emit.Seq(graph.Nodes[index], continueNode);
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

            var breakNode = Emit.Break();
            if (children.Length != 2)
                throw new ControlFlowGraphException($"Break at {index} has unexpected child count ({children.Length})", graph);

            bool isfirstChildInLoop = loop.Body.Any(x => x.Index == children[0]);
            var target = isfirstChildInLoop ? children[1] : children[0];

            if (target != loop.MainExit)
            {
                var targetChildren = graph.Children(target);
                if (targetChildren.Length != 1 || targetChildren[0] != loop.MainExit)
                    return graph;

                bool label = graph.GetEdgeLabel(index, target);

                var condition = graph.Nodes[index];
                if (!label)
                    condition = Emit.Negation(condition);

                var newBlock = Emit.Seq(graph.Nodes[target], Emit.Break());
                var ifNode = Emit.If(condition, newBlock);
                return graph
                    .RemoveNode(target)
                    .ReplaceNode(index, ifNode);
            }

            return ReplaceLoopBranch(graph, index, target, breakNode);
        }

        static ControlFlowGraph ReplaceLoopBranch(ControlFlowGraph graph, int index, int target, ICfgNode newNode)
        {
            bool label = graph.GetEdgeLabel(index, target);
            var condition = graph.Nodes[index];

            if (!label)
                condition = Emit.Negation(condition);

            var ifNode = Emit.If(condition, newNode);
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
                return ReplaceLoopBranch(graph, tail, target, Emit.Break());
            }

            var newNode = Emit.While(graph.Nodes[head], graph.Nodes[tail]);
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

            var newNode = Emit.Do(graph.Nodes[tail], graph.Nodes[head]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        public static ControlFlowGraph ReduceSeseRegions(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var regions = graph.GetAllSeseRegions();

            // Do smallest regions first, as they may be nested in a larger one
            foreach (var (region, regionHead) in regions.OrderBy(x => x.nodes.Count))
            {
                if (regionHead == graph.HeadIndex)
                    continue; // Don't try and reduce the 'sequence' of start node -> actual entry point nodes when there's only one entry

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
                bool containsOther = regions.Any(x => x.nodes != region && !x.nodes.Except(region).Any());
                if (containsOther)
                    continue;

                var cut = graph.Cut(region, regionHead);

                if (cut.Cut.IsCyclic())
                    continue; // Must be some sort of weird loop that couldn't be reduced earlier, currently irreducible.

                var restructured = SeseReducer.Reduce(cut.Cut, DummyLabelPrefix);
                int restructuredHead = restructured.HeadIndex;
                int restructuredTail = cut.Cut.GetExitNode();

                var (updated, mapping) = cut.Remainder.Merge(restructured);

                foreach (var (start, _, label) in cut.RemainderToCutEdges)
                    updated = updated.AddEdge(start, mapping[restructuredHead], label);

                foreach (var (_, end, label) in cut.CutToRemainderEdges)
                    updated = updated.AddEdge(mapping[restructuredTail], end, label);

                return updated;
            }

            return graph;
        }
    }
}
#pragma warning restore 8321