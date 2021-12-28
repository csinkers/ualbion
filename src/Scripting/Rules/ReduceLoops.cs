using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting.Rules;

public static class ReduceLoops
{
    public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        var loops = GetLoops(graph);

        // Add an empty node for the header so the header will never be a continue / break. Then we can structure 
        // all loops as infinite loops (the most general case) and use a separate step to identify while / do loops.
        foreach (var index in graph.GetDfsPostOrder())
        {
            foreach (var loop in loops)
            {
                if (loop.Header.Index != index)
                    continue;

                var headerNode = graph.Nodes[loop.Header.Index];
                if (headerNode is EmptyNode)
                    continue;

                int? successor = null;
                foreach (var child in graph.Children(loop.Header.Index).Where(x => graph.GetEdgeLabel(loop.Header.Index, x) == CfgEdge.LoopSuccessor))
                    successor = child;

                graph = graph.InsertBefore(loop.Header.Index, Emit.Empty(), out var newHeaderIndex);

                if (successor.HasValue)
                {
                    graph = graph
                        .RemoveEdge(loop.Header.Index, successor.Value)
                        .AddEdge(newHeaderIndex, successor.Value, CfgEdge.LoopSuccessor);
                }

                return (graph, "Add empty node for loop header");
            }
        }

        foreach (var index in graph.GetDfsPostOrder())
        {
            foreach (var loop in loops)
            {
                if (loop.Header.Index != index) // Handle non-header exits and back edges (post-order iteration means these will be handled before the header)
                {
                    var part = loop.Body.FirstOrDefault(x => x.Index == index);
                    if (part == null) 
                        continue;

                    // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
                    var updated = ReduceContinue(graph, loop, part);
                    if (updated != graph)
                        return (updated, "Reduce continue");

                    updated = ReduceBreak(graph, loop, part);
                    if (updated != graph)
                        return (updated, "Reduce break");
                }
                else // Handle header
                {
                    if (loop.Body.Count != 1 || loop.Body[0].OutsideEntry) // TODO: Add a separate reducer to turn outside entries into gotos + labels
                        continue;

                    int tail = loop.Body[0].Index;
                    var loopNode = Emit.Loop(Emit.Seq(graph.Nodes[index], graph.Nodes[tail]));
                    return (graph
                        .RemoveNode(tail)
                        .ReplaceNode(index, loopNode), "Reduce generic loop");
                }
            }
        }
        return (graph, null);
    }

    static ControlFlowGraph ReduceContinue(ControlFlowGraph graph, CfgLoop loop, LoopPart part)
    {
        // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation

        // Outside entry = non-structured code, so give up
        // Tail = the tail of a loop always continues by default, don't need a statement - the top-level loop reduction will handle it.
        // Header = continuing header should be handled by simple loop rule
        // !Continue = if it's not a jump back to the header, it's not a continue
        // Break = shouldn't be both a continue and a break
        if (part.OutsideEntry || part.Tail || part.Header || !part.Continue || part.Break)
            return graph;

        var children = graph.Children(part.Index);
        switch (children.Length)
        {
            case 1:
            {
                var seq = Emit.Seq(graph.Nodes[part.Index], Emit.Continue());
                var tailIndex = loop.Body.First(x => x.Tail).Index;
                return graph
                    .ReplaceNode(part.Index, seq)
                    .RemoveEdge(part.Index, loop.Header.Index)
                    .AddEdge(part.Index, tailIndex, CfgEdge.True);
            }

            case 2:
                return ReplaceLoopBranch(graph, part.Index, loop.Header.Index, Emit.Continue());

            default:
                throw new ControlFlowGraphException($"Continue at {part.Index} has unexpected child count ({children.Length})", graph);
        }
    }

    static ControlFlowGraph ReduceBreak(ControlFlowGraph graph, CfgLoop loop, LoopPart part)
    {
        // Func<string> vis = () => graph.ToVis().AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation

        // Break = needs to exit the loop to be a break
        if (!part.Break)
            return graph;

        // Outside entry = non-structured code, so give up
        if (part.OutsideEntry)
            return graph;

        var children = graph.Children(part.Index);
             
        if (children.Length != 2)
            throw new ControlFlowGraphException($"Break at {part.Index} has unexpected child count ({children.Length})", graph);

        bool isfirstChildInLoop = loop.Body.Any(x => x.Index == children[0]) || children[0] == loop.Header.Index;
        var exitTarget = isfirstChildInLoop ? children[1] : children[0];

        // Add LoopSuccessor edge if this is the last link to the MainExit.
        var remainingLoopChildren = loop.Body
            .Where(x => x.Index != part.Index)
            .Aggregate(
                (IEnumerable<int>)graph.Children(loop.Header.Index),
                (current, x) => current.Union(graph.Children(x.Index)));

        if (loop.MainExit.HasValue && !remainingLoopChildren.Contains(loop.MainExit.Value))
            graph = graph.AddEdge(loop.Header.Index, loop.MainExit.Value, CfgEdge.LoopSuccessor);

        if (exitTarget != loop.MainExit)
        {
            var targetChildren = graph.Children(exitTarget);
            if (targetChildren.Length != 1 || targetChildren[0] != loop.MainExit)
                return graph;

            var condition = graph.Nodes[part.Index];
            if (graph.GetEdgeLabel(part.Index, exitTarget) == CfgEdge.False)
                condition = Emit.Negation(condition);

            var ifNode = Emit.If(condition, Emit.Seq(graph.Nodes[exitTarget], Emit.Break()));
            return graph
                .RemoveNode(exitTarget)
                .ReplaceNode(part.Index, ifNode);
        }

        return ReplaceLoopBranch(graph, part.Index, exitTarget, Emit.Break());
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

    public static IList<CfgLoop> GetLoops(ControlFlowGraph graph)
    {
        var components = graph.GetStronglyConnectedComponents();
        var loops =
            from component in components.Where(x => x.Count > 1)
            from loop in graph.GetAllSimpleLoops(component)
            select GetLoopInformation(graph, loop);

        return loops.ToList();
    }

    public static CfgLoop GetLoopInformation(ControlFlowGraph graph, List<int> nodes)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (nodes.Count == 0) throw new ArgumentException("Empty loop provided to GetLoopInformation", nameof(nodes));

        var body = new List<LoopPart>();
        var header = new LoopPart(nodes[0], true);
        var exits = new HashSet<int>();

        // Determine if header can break out of the loop
        foreach (int child in graph.Children(nodes[0]))
        {
            if (nodes.Contains(child))
                continue;

            CfgEdge edgeLabel = graph.GetEdgeLabel(nodes[0], child);
            if (edgeLabel == CfgEdge.LoopSuccessor) // Loop successor pseudo-edges don't count for break-detection
                continue;

            header = new LoopPart(header.Index, true, Break: true, Negated: edgeLabel == CfgEdge.True);
            exits.Add(child);
        }

        for (int i = 1; i < nodes.Count; i++)
        {
            var node = nodes[i];
            bool isContinue = false;
            bool isBreak = false;
            bool isTail = true;
            bool negated = false;

            foreach (int child in graph.Children(node))
            {
                // Func<string> vis = () => ToVis().AddPointer("i", node).AddPointer("child", child).ToString(); // For VS Code debug visualisation

                if (child == header.Index) // Jump to header = possible continue
                    isContinue = true;
                else if (nodes.Contains(child))
                    isTail = false;
                else
                {
                    negated = graph.GetEdgeLabel(node, child) == CfgEdge.False;
                    isBreak = true;
                    exits.Add(child);
                }
            }

            bool hasOutsideEntry = Enumerable.Any(graph.Parents(node), x => !nodes.Contains(x));
            body.Add(new LoopPart(node, false, isTail, isBreak, isContinue, hasOutsideEntry, negated));
        }

        var postDom = graph.GetPostDominatorTree();
        int? mainExit = postDom.ImmediateDominator(header.Index);
        while (mainExit.HasValue && body.Any(x => !postDom.Dominates(mainExit.Value, x.Index)))
            mainExit = postDom.ImmediateDominator(mainExit.Value);

        if (body.Count(x => x.Tail) > 1) // Only allow one tail, pick one of the nodes with the longest path from the header.
        {
            var longestPaths = new Dictionary<int, int>();
            for (int i = 0; i < body.Count; i++)
            {
                var part = body[i];
                if (!part.Tail)
                    continue;
                var paths = graph.GetAllReachingPaths(header.Index, part.Index);
                longestPaths[i] = paths.Select(x => x.Count).Max();
            }

            var longestDistance = longestPaths.Values.Max();
            var winner = longestPaths.First(x => x.Value == longestDistance).Key;
            foreach(var kvp in longestPaths)
            {
                if (kvp.Key == winner) continue;
                var part = body[kvp.Key];
                body[kvp.Key] = new LoopPart(part.Index, part.Header, false, part.Break, part.Continue, part.OutsideEntry, part.Negated);
            }
        }

        return new CfgLoop(header, body.ToImmutableList(), exits.ToImmutableList(), mainExit);
    }
}