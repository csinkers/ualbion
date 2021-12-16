using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Scripting.Ast;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Scripting
{
    public static class Decompiler
    {
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
                results.Add(SimplifyGraph(graph, record));
            }

            return results;
        }

        public static ICfgNode SimplifyGraph(ControlFlowGraph graph, RecordFunc record)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            record("Begin decompilation", graph);
            // Func<string> vis = () => graph.Visualize(); // For VS Code debug visualisation
            ControlFlowGraph previous = null;
            int iterations = 0;
            while (graph != previous)
            {
                previous = graph;
                iterations++;
                if (iterations > 1000)
                    throw new ControlFlowGraphException("Iteration overflow, assuming graph cannot be structured", graph);

                graph = SimplifyOnce(graph, record);
            }

            graph = record("Defragment", graph.Defragment());
            if (graph.ActiveNodeCount != 1)
                throw new ControlFlowGraphException("Could not reduce graph to an AST", graph);

            graph = record("Relabel", CfgRelabeller.Relabel(graph, ScriptConstants.DummyLabelPrefix));
            graph = record("Remove empty nodes", graph.AcceptBuilder(new EmptyNodeRemovalVisitor()));
            return graph.Entry;
        }

        public static ControlFlowGraph SimplifyOnce(ControlFlowGraph previous, RecordFunc record)
        {
            var graph = previous;
            // graph = record("Defragment", graph.Defragment());

            graph = record("Connect disjoint node to exit", ConnectDisjointNodeToExit(graph));
            if (graph != previous)
                return graph;

            graph = record("Reduce simple while", ReduceSimpleWhile(graph));
            if (graph != previous)
                return graph;

            graph = record("Reduce sequence", ReduceSequences(graph));
            if (graph != previous)
                return graph;

            graph = record("Reduce if-then", ReduceIfThen(graph));
            if (graph != previous)
                return graph;

            graph = record("Reduce if-then-else", ReduceIfThenElse(graph));
            if (graph != previous)
                return graph;

            graph = record("Reduce SESE region", ReduceSeseRegions(graph));
            if (graph != previous)
                return graph;

            // graph = record("Reduce loops", ReduceLoops(graph, record));
            // if (graph != previous)
            //     return graph;

            graph = record("Reduce loop parts", ReduceLoopParts(graph));
            if (graph != previous)
                return graph;

            // graph = record("Reduce simple loop", ReduceSimpleLoops(graph));
            // if (graph != previous)
            //     return graph;

            return graph;
        }

        static ControlFlowGraph ConnectDisjointNodeToExit(ControlFlowGraph graph)
        {
            var reachability = graph.Reverse().GetReachability(graph.ExitIndex, out var reachableCount);
            if (reachableCount == graph.ActiveNodeCount)
                return graph;

            var acyclic = graph.RemoveBackEdges();
            var distances = acyclic.GetLongestPaths();
            int longestDistance = 0;
            int winner = -1;

            for (int i = 0; i < graph.NodeCount; i++)
            {
                if (graph.Nodes[i] == null || reachability[i]) // Only consider nodes that can't reach the exit
                    continue;

                if (distances[i] <= longestDistance) continue;
                longestDistance = distances[i];
                winner = i;
            }

            return graph.AddEdge(winner, graph.ExitIndex, CfgEdge.DisjointGraphFixup);
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

                var edges = new List<(int, int, CfgEdge)>();
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
                        edges.Add((i, node.Next.Id, CfgEdge.True));
                    }
                    else edges.Add((i, exit, CfgEdge.True));

                    if (node is IBranchNode branch && !ReferenceEquals(branch.NextIfFalse, branch.Next))
                    {
                        if (branch.NextIfFalse != null)
                        {
                            queue.Enqueue(branch.NextIfFalse);
                            edges.Add((i, branch.NextIfFalse.Id, CfgEdge.False));
                        }
                        else edges.Add((i, exit, CfgEdge.False));
                    }
                }
                edges.Add((entry, head, CfgEdge.True));

                var graph = new ControlFlowGraph(entry, exit, nodes, edges);
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

        public static ControlFlowGraph ReduceSimpleWhile(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var simpleLoopIndices =
                from index in graph.GetDfsPostOrder()
                let children = graph.Children(index)
                where children.Length is 1 or 2 && children.Contains(index)
                select index;

            foreach (var index in simpleLoopIndices)
            {
                // Func<string> vis = () => graph.ToVis().AddPointer("index", index).ToString(); // For VS Code debug visualisation
                if (graph.Children(index).Length == 1)
                    return ReduceEmptyInfiniteWhileLoop(graph, index);

                var condition = graph.Nodes[index];
                if (graph.GetEdgeLabel(index, index) == CfgEdge.False)
                    condition = Emit.Negation(condition);

                var updated = graph
                    .ReplaceNode(index, Emit.While(condition, null))
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
                // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("child", child).ToString(); // For VS Code debug visualisation

                var childsParents = graph.Parents(child);
                var grandChildren = graph.Children(child);

                if (childsParents.Length != 1 || grandChildren.Length > 1)
                    continue; // Is a jump target from somewhere else as well - can't combine

                if (grandChildren.Length == 1 && (grandChildren[0] == index || grandChildren[0] == child))
                    continue; // Loops around, not a sequence

                var node = graph.Nodes[index];
                var childNode = graph.Nodes[child];

                var updated = graph
                    .RemoveNode(child)
                    .ReplaceNode(index, Emit.Seq(node, childNode));

                foreach (var grandChild in grandChildren)
                    updated = updated.AddEdge(index, grandChild, graph.GetEdgeLabel(child, grandChild));

                return updated;
            }

            return graph;
        }

        public static ControlFlowGraph ReduceIfThen(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            foreach (var head in graph.GetDfsPostOrder())
            {
                var children = graph.Children(head);
                if (children.Length != 2) 
                    continue;

                int after = -1;
                var then = -1;
                // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("after", after).AddPointer("then", then).ToString(); // For VS Code debug visualisation

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

                var label = graph.GetEdgeLabel(head, then);
                var condition = label == CfgEdge.False ? Emit.Negation(graph.Nodes[head]) : graph.Nodes[head];

                var newNode = Emit.If(condition, graph.Nodes[then]);
                return graph.RemoveNode(then).ReplaceNode(head, newNode);
            }

            return graph;
        }

        public static ControlFlowGraph ReduceIfThenElse(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            foreach (var head in graph.GetDfsPostOrder())
            {
                var children = graph.Children(head);
                if (children.Length != 2) 
                    continue;

                var left = children[0];
                var right = children[1];
                // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("after", after).AddPointer("left", left).AddPointer("right", right).ToString(); // For VS Code debug visualisation

                var leftParents = graph.Parents(left);
                var rightParents = graph.Parents(right);
                var leftChildren = graph.Children(left);
                var rightChildren = graph.Children(right);

                if (leftParents.Length != 1 || rightParents.Length != 1) // Branches of an if can't be jump destinations from elsewhere
                    continue;

                bool isRegularIfThenElse =
                    leftChildren.Length == 1 && rightChildren.Length == 1 &&
                    leftChildren[0] == rightChildren[0];

                bool isTerminalIfThenElse = leftChildren.Length == 0 && rightChildren.Length == 0;

                if (!isRegularIfThenElse && !isTerminalIfThenElse)
                    continue;

                var leftLabel = graph.GetEdgeLabel(head, left);
                var thenIndex = leftLabel == CfgEdge.False ? right : left;
                var elseIndex = leftLabel == CfgEdge.False ? left : right;
                var after = isRegularIfThenElse ? leftChildren[0] : -1;

                if (after == head)
                    continue;

                var newNode = Emit.IfElse(
                    graph.Nodes[head],
                    graph.Nodes[thenIndex],
                    graph.Nodes[elseIndex]);

                var updated = graph;
                if (isRegularIfThenElse)
                    updated = updated.AddEdge(head, after, CfgEdge.True);

                return updated
                    .RemoveNode(thenIndex)
                    .RemoveNode(elseIndex)
                    .ReplaceNode(head, newNode);
            }

            return graph;
        }

        static bool DominatesAll(DominatorTree tree, int dominator, IEnumerable<int> indices)
        {
            foreach (var index in indices)
                if (!tree.Dominates(dominator, index))
                    return false;
            return true;
        }

        static ControlFlowGraph ReduceLoops(ControlFlowGraph graph, RecordFunc record)
        {
            var loops = graph.GetLoops();
            foreach (var loop in loops.OrderBy(x => x.Body.Count))
            {
                // Find pre-dominator
                var dom = graph.GetDominatorTree();
                if (!DominatesAll(dom, loop.Header.Index, loop.Body.Select(x => x.Index)))
                    continue;

                // Find post-dominator
                var postdom = graph.GetPostDominatorTree();
                var loopExit = postdom.ImmediateDominator(loop.Header.Index);
                if (!loopExit.HasValue || !DominatesAll(postdom, loopExit.Value, loop.Body.Select(x => x.Index)))
                    continue;

/*
                // Isolate edges to post-dominator via an empty node (if necessary)
                HashSet<int> region = new();
                graph.GetRegionParts(region, loop.Header.Index, loopExit.Value);
                region.Add(loopExit.Value);

                // Cut out loop and hand off to loop simplifier.
                var cut = graph.Cut(region, loop.Header.Index, loopExit.Value);
                var simplified = LoopSimplifier.SimplifyLoop(cut.Cut, record);
                if (simplified == null || simplified == cut.Cut)
                    continue;

                return cut.Merge(simplified);
*/
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
                    {
                        // Grow loop then reduce
                        continue;
                    }

                    foreach (var part in loop.Body)
                    {
                        // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
                        if (part.Index != index)
                            continue;

                        var updated = ReduceContinue(part, graph, index, loop);
                        if (updated != graph)
                            return updated;

                        updated = ReduceBreak(part, graph, index, loop);
                        if (updated != graph)
                            return updated;
                    }

                    if (loop.Header.Index != index
                     || loop.IsMultiExit
                     || loop.Body.Count != 1
                     || loop.Body[0].OutsideEntry)
                    {
                        continue;
                    }

                    int tail = loop.Body[0].Index;
                    if (loop.Header.Break)
                    {
                        var updated = ReduceWhileLoop(graph, index, tail, loop.Header.Negated);
                        if (updated != graph)
                            return updated;
                    }
                    else if (loop.Body.All(x => !x.Break)) // Infinite while loop
                    {
                        var updated = ReduceInfiniteWhileLoop(graph, index, tail);
                        if (updated != graph)
                            return updated;
                    }
                    else
                    {
                        var updated = ReduceDoLoop(graph, index, tail);
                        if (updated != graph)
                            return updated;
                    }
                }
            }
            return graph;
        }

        static ControlFlowGraph ReduceContinue(LoopPart part, ControlFlowGraph graph, int index, CfgLoop loop)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
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
            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
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

                var condition = graph.Nodes[index];
                if (graph.GetEdgeLabel(index, target) == CfgEdge.False)
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
            var label = graph.GetEdgeLabel(index, target);
            var condition = graph.Nodes[index];

            if (label == CfgEdge.False)
                condition = Emit.Negation(condition);

            var ifNode = Emit.If(condition, newNode);
            return graph
                .ReplaceNode(index, ifNode)
                .RemoveEdge(index, target);
        }

        static ControlFlowGraph ReduceEmptyInfiniteWhileLoop(ControlFlowGraph graph, int index)
        {
            var exitNode = GetFirstEmptyExitNode(graph);
            var label = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var subgraph = new ControlFlowGraph(new[]
                {
                    Emit.Empty(), // 0
                    Emit.Label(label), // 1
                    graph.Nodes[index], // 2
                    Emit.Goto(label), // 3
                    Emit.Empty(), // 4
                },
                new[] { (0,1,CfgEdge.True), (1,2,CfgEdge.True), (2,3,CfgEdge.True), (3,4,CfgEdge.True), });

            return graph
                .AddEdge(index, exitNode, CfgEdge.True)
                .ReplaceNode(index, subgraph);
        }

        static ControlFlowGraph ReduceInfiniteWhileLoop(ControlFlowGraph graph, int head, int tail)
        {
            var exitNode = GetFirstEmptyExitNode(graph);
            var label = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var subgraph = new ControlFlowGraph(new[]
                {
                    Emit.Empty(), // 0
                    Emit.Label(label), // 1
                    graph.Nodes[head], // 2
                    graph.Nodes[tail], // 3
                    Emit.Goto(label), // 4
                    Emit.Empty(), // 5
                },
                new[] { (0,1,CfgEdge.True), (1,2,CfgEdge.True), (2,3,CfgEdge.True), (3,4,CfgEdge.True), (4,5,CfgEdge.True), });

            return graph
                .RemoveNode(tail)
                .AddEdge(head, exitNode, CfgEdge.True)
                .ReplaceNode(head, subgraph);
        }

        static ControlFlowGraph ReduceWhileLoop(ControlFlowGraph graph, int head, int tail, bool negated)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("tail", tail).ToString(); // For VS Code debug visualisation
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 1)
            {
                bool isfirstChildInLoop = head == children[0];
                var target = isfirstChildInLoop ? children[1] : children[0];
                return ReplaceLoopBranch(graph, tail, target, Emit.Break());
            }

            var condition = graph.Nodes[head];
            if (negated)
                condition = Emit.Negation(condition);

            var newNode = Emit.While(condition, graph.Nodes[tail]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        static ControlFlowGraph ReduceDoLoop(ControlFlowGraph graph, int head, int tail)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("tail", tail).ToString(); // For VS Code debug visualisation
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 2)
                return updated;

            foreach (int child in children)
                if (child != head)
                    updated = updated.AddEdge(head, child, CfgEdge.True);

            var condition = graph.Nodes[tail];
            if (graph.GetEdgeLabel(tail, head) == CfgEdge.False)
                condition = Emit.Negation(condition);

            var newNode = Emit.Do(condition, graph.Nodes[head]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        public static ControlFlowGraph ReduceSeseRegions(ControlFlowGraph graph/*, RecordFunc recordFunc = null*/)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var regions = graph.GetAllSeseRegions();

            // Do smallest regions first, as they may be nested in a larger one
            foreach (var (region, regionEntry, regionExit) in regions.OrderBy(x => x.nodes.Count))
            {
                if (regionEntry == graph.EntryIndex)
                    continue; // Don't try and reduce the 'sequence' of start node -> actual entry point nodes when there's only one entry

                /* Func<string> vis = () => // For VS Code debug visualisation
                {
                     var d = graph.ToVis(); 
                     foreach(var n in d.Nodes)
                        if (region.Contains(int.Parse(n.Id, CultureInfo.InvariantCulture)))
                            n.Color = "#4040b0";
                     return d.ToString();
                }; */
                bool containsOther = regions.Any(x => x.nodes != region && !x.nodes.Except(region).Any());
                if (containsOther)
                    continue;

                var cut = graph.Cut(region, regionEntry, regionExit);

                if (cut.Cut.IsCyclic()) // Loop reduction comes later
                    continue;

                return cut.Merge(SeseReducer.Reduce(cut.Cut));
            }

            return graph;
        }

        static int GetFirstEmptyExitNode(ControlFlowGraph graph)
        {
            int exitNode = -1;
            foreach (var candidate in graph.GetExitNodes())
                if (graph.Nodes[candidate] is EmptyNode)
                    exitNode = candidate;

            if (exitNode == -1)
                throw new ControlFlowGraphException("Could not structure infinite loop", graph);

            return exitNode;
        }

        public static ControlFlowGraph BreakEdge(ControlFlowGraph graph, int start, int end, ICfgNode sourceNode, string destLabel)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));
            var label = graph.GetEdgeLabel(start, end);
            graph = graph
                .RemoveEdge(start, end)
                .AddNode(sourceNode, out var sourceNodeIndex)
                .AddEdge(start, sourceNodeIndex, label)
                .AddEdge(sourceNodeIndex, graph.ExitIndex, CfgEdge.True);

            if (destLabel != null)
            {
                graph = graph.ReplaceNode(end,
                    new ControlFlowGraph(0, 1,
                        new[] { Emit.Label(destLabel), graph.Nodes[end] },
                        new[] { (0, 1, CfgEdge.True) }));
            }

            return graph;
        }
    }
}
#pragma warning restore 8321