using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Scripting.Ast;
using UAlbion.Scripting.Rules;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Scripting
{
    public static class Decompiler
    {
        public static readonly List<ControlFlowRuleDelegate> DefaultRules = new()
        {
            ConnectDisjointNodeToExit.Decompile,
            ReduceSimpleWhile.Decompile,
            ReduceSequences.Decompile,
            ReduceIfThen.Decompile,
            ReduceIfThenElse.Decompile,
            ReduceSeseRegions.Decompile,
            ReduceLoops.Decompile,
            x => (x.Defragment(), "Defragment"),
            x => (CfgRelabeller.Relabel(x, ScriptConstants.DummyLabelPrefix), "Relabel"),
            x => (x.AcceptBuilder(new EmptyNodeRemovalVisitor()), "Remove empty nodes")
        };

        public static List<ICfgNode> Decompile<T>(
            IList<T> nodes,
            IEnumerable<ushort> chains,
            IEnumerable<ushort> additionalEntryPoints,
            List<(string, IGraph)> steps = null,
            IList<ControlFlowRuleDelegate> rules = null) where T : IEventNode
        {
            rules ??= DefaultRules;
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

        public static ICfgNode SimplifyGraph(ControlFlowGraph graph, RecordFunc record, IList<ControlFlowRuleDelegate> rules = null)
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

                graph = SimplifyOnce(graph, record, rules);
            }

            if (graph.ActiveNodeCount > 1)
                throw new ControlFlowGraphException("Could not structure graph", graph);

            return graph.Entry;
        }

        public static ControlFlowGraph SimplifyOnce(ControlFlowGraph previous, RecordFunc record, IList<ControlFlowRuleDelegate> rules = null)
        {
            rules ??= DefaultRules;

            var graph = previous;
            foreach (var rule in rules)
            {
                string description;
                (graph, description) = rule(graph);
                graph = record(description, graph);
                if (graph != previous)
                    return graph;
            }

            return graph;
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
                foreach (var chainHead in chains)
                    Visit(chainHead, $"Chain{index++}");
            }

            if (additionalEntryPoints != null)
                foreach (var head in additionalEntryPoints)
                    Visit(head, $"Event{head}");

            for (var i = 0; i < results.Count; i++)
                results[i] = results[i].Defragment();

            return results;
        }
    }
}
#pragma warning restore 8321