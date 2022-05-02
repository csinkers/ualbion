using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;
using UAlbion.Scripting.Rules;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Scripting;

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
        SimplifyLabels.Apply,
        RemoveEmptyNodes.Apply,
        LoopConverter.Apply,
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
        var sw = Stopwatch.StartNew();
        if (steps != null)
        {
            var steps2 = steps;
            record = (description, graph) =>
            {
                if (steps2.Count == 0 || steps[^1].Item2 != graph)
                {
                    var ms = sw.ElapsedMilliseconds;
                    steps2.Add(ms == 0
                        ? (description, graph)
                        : ($"{description} ({sw.ElapsedMilliseconds} ms)", graph));
                    sw.Restart();
                }

                return graph;
            };
        }
        else record = (_, x) => x;

        var graphs = BuildEventRegions(nodes, chains, additionalEntryPoints);
        var results = new List<ICfgNode>();
        for (var index = 0; index < graphs.Count; index++)
        {
            var graph = record($"Make region {index}", graphs[index]);
            results.Add(SimplifyGraph(graph, record, rules));
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

    public static IList<ControlFlowGraph> BuildEventRegions<T>(
        IList<T> events,
        IEnumerable<ushort> chains,
        IEnumerable<ushort> additionalEntryPoints) where T : IEventNode
    {
        chains ??= Enumerable.Empty<ushort>();
        additionalEntryPoints ??= Enumerable.Empty<ushort>();

        var graph = BuildDisconnectedGraphFromEvents(events);
        var results = new List<ControlFlowGraph>();

        // Create a graph per chain entry
        int chainId = 0;
        foreach (var chainEntry in chains)
        {
            try
            {
                if (chainEntry == 0xffff) // Terminal value for unused chains etc
                    continue;

                var subGraph = BuildSubGraph(chainEntry, ScriptConstants.BuildChainLabel(chainId), graph, events);
                results.Add(subGraph);
            }
            finally { chainId++; }
        }

        // Create a graph per extra entry
        foreach (var additionalEntryPoint in additionalEntryPoints)
        {
            var subGraph = BuildSubGraph(additionalEntryPoint, ScriptConstants.BuildAdditionalEntryLabel(additionalEntryPoint), graph, events);
            results.Add(subGraph);
        }

        return results;
    }

    static ControlFlowGraph BuildSubGraph<T>(int start, string label, ControlFlowGraph graph, IList<T> events) where T : IEventNode
    {
        var nodes = graph.Nodes;
        var entry = graph.EntryIndex;
        var exit = graph.ExitIndex;
        var activeIds = graph.GetDfsOrder(start, false).ToHashSet();
        var subset = new ICfgNode[nodes.Count];
        for (int i = 0; i < subset.Length; i++)
            if (activeIds.Contains(i) || i == entry || i == exit)
                subset[i] = nodes[i];

        var edges = graph.LabelledEdges.Where(x => activeIds.Contains(x.start));
        var trueExits = events.Where(x => activeIds.Contains(x.Id) && x.Next == null).Select(x => ((int)x.Id, exit, CfgEdge.True));
        var falseExits = events.OfType<IBranchNode>().Where(x => activeIds.Contains(x.Id) && x.NextIfFalse == null).Select(x => ((int)x.Id, exit, CfgEdge.False));
        edges = edges.Concat(trueExits).Concat(falseExits).Append((entry, start, CfgEdge.True));
        edges = FilterOutDuplicateEdges(edges);
        var result = new ControlFlowGraph(subset, edges);
        return result.InsertBefore(start, Emit.Label(label));
    }

    static IEnumerable<(int start, int end, CfgEdge label)> FilterOutDuplicateEdges(IEnumerable<(int start, int end, CfgEdge label)> edges)
    {
        var result = new Dictionary<(int start, int end), CfgEdge>();
        foreach (var (start, end, label) in edges)
        {
            if (result.TryGetValue((start, end), out var existingLabel))
                if (existingLabel == CfgEdge.True && label == CfgEdge.False)
                    continue; // True overrides false

            result[(start, end)] = label;
        }

        return result.Select(x => (x.Key.start, x.Key.end, x.Value));
    }

    public static ControlFlowGraph BuildDisconnectedGraphFromEvents<T>(IList<T> events) where T : IEventNode
    {
        var nodes = events.Select(x => (ICfgNode)Emit.Event(x.Event)).ToList();

        // Add empty nodes for the unique entry/exit points
        var entry = nodes.Count;
        nodes.Add(Emit.Empty());
        var exit = nodes.Count;
        nodes.Add(Emit.Empty());

        var trueEdges = events.Where(x => x.Next != null).Select(x => ((int)x.Id, (int)x.Next.Id, CfgEdge.True));
        var falseEdges = events.OfType<IBranchNode>().Where(x => x.NextIfFalse != null).Select(x => ((int)x.Id, (int)x.NextIfFalse.Id, CfgEdge.False));
        var edges = trueEdges.Concat(falseEdges);
        edges = FilterOutDuplicateEdges(edges);
        return new ControlFlowGraph(entry, exit, nodes, edges);
    }

    /* // Split into regions preserving shared tails - requires fancier handling during later decompilation steps
    public static IList<ControlFlowGraph> BuildEventRegions<T>(
        IList<T> events,
        IEnumerable<ushort> chains,
        IEnumerable<ushort> additionalEntryPoints) where T : IEventNode
    {
        chains ??= Enumerable.Empty<ushort>();
        additionalEntryPoints ??= Enumerable.Empty<ushort>();

        var graph = BuildDisconnectedGraphFromEvents(events);
        var nodes = graph.Nodes;
        var entry = graph.EntryIndex;
        var exit = graph.ExitIndex;
        var (components, componentCount) = graph.GetComponentMapping();
        var validComponentCount = componentCount - 2; // Last two components are just entry / exit nodes

        // Copy across valid nodes/edges for each component
        var nodeSets = Enumerable.Repeat(0, validComponentCount).Select(_ => new ICfgNode[nodes.Count]).ToArray();
        for (var i = 0; i < nodes.Count - 2; i++) // -2 to exclude the entry/exit nodes
        {
            var ci = components[i];
            nodeSets[ci][i] = nodes[i];
        }

        var edgeSets = Enumerable.Repeat(0, validComponentCount).Select(_ => new List<(int, int, CfgEdge)>()).ToArray();
        foreach (var (start, end, label) in graph.LabelledEdges)
        {
            var ci = components[start];
            edgeSets[ci].Add((start, end, label));
        }

        // Add entry/exit nodes to each component
        for (int ci = 0; ci < validComponentCount; ci++)
        {
            nodeSets[ci][entry] = nodes[entry];
            nodeSets[ci][exit] = nodes[exit];
        }

        // Add exit edges
        var trueExits = events.Where(x => x.Next == null).Select(x => ((int)x.Id, exit, CfgEdge.True));
        var falseExits = events.OfType<IBranchNode>().Where(x => x.NextIfFalse == null).Select(x => ((int)x.Id, exit, CfgEdge.False));
        var exitEdges = trueExits.Concat(falseExits);
        foreach (var exitEdge in exitEdges)
        {
            var ci = components[exitEdge.Item1];
            edgeSets[ci].Add(exitEdge);
        }

        // Build graphs
        var results = nodeSets.Zip(edgeSets, (n, e) => new ControlFlowGraph(entry, exit, n, e)).ToArray();

        // Add entry links and labels for chains
        int chainId = 0;
        foreach (var chainStart in chains)
        {
            if (chainStart == 0xffff) // Terminal value for unused chains etc
                continue;

            var ci = components[chainStart];
            results[ci] =
                results[ci]
                .AddEdge(entry, chainStart, CfgEdge.EntryPoint) // Use special edge label to prevent the if/else reducers applying during decompilation
                .InsertBefore(chainStart, Emit.Label($"Chain{chainId}"));

            chainId++;
        }

        // Add entry links and labels for extra entries
        foreach (var extraStart in additionalEntryPoints)
        {
            var ci = components[extraStart];
            results[ci] =
                results[ci]
                .AddEdge(entry, extraStart, CfgEdge.EntryPoint)
                .InsertBefore(extraStart, Emit.Label($"Event{extraStart}"));

            chainId++;
        }

        // Clear out unused nodes
        for (int ci = 0; ci < results.Length; ci++)
            results[ci] = results[ci].Defragment(true);

        return results;
    } //*/

    /* // First attempt, doesn't handle shared suffixes
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
            results[i] = results[i].Defragment(true);

        return results;
    } //*/
}
#pragma warning restore 8321