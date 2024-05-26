using System;
using System.Linq;

namespace UAlbion.Scripting.Rules;

public static class ReduceSeseRegions
{
    const string Description = "Reduce SESE region";
    public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph/*, RecordFunc recordFunc = null*/)
    {
        ArgumentNullException.ThrowIfNull(graph);
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
                    if (region.Contains(int.Parse(n.Id)))
                        n.Color = "#4040b0";
                 return d.ToString();
            }; */
            bool containsOther = regions.Any(x => x.nodes != region && !x.nodes.Except(region).Any());
            if (containsOther)
                continue;

            // If either end is a loop header then leave it alone and let the loop rule handle it.
            if (graph.GetBackEdges().Any(x => x.end == regionEntry || x.end == regionExit))
                continue;

            var cut = graph.Cut(region, regionEntry, regionExit);

            if (cut.Cut.IsCyclic()) // Loop reduction comes later
                continue;

            return (cut.Merge(ReduceSese(cut.Cut)), Description);
        }

        return (graph, null);
    }

    static ControlFlowGraph ReduceSese(ControlFlowGraph cut)
    {
        var minDepth = cut.GetShortestPaths(cut.EntryIndex);
        var maxDepth = cut.GetLongestPaths(cut.EntryIndex);
        var exitNode = cut.GetExitNode();

        int maxDelta = 0;
        int maxDeltaNode = -1;
        for (int i = 0; i < cut.Nodes.Count; i++)
        {
            if (i == exitNode)
                continue;

            var delta = maxDepth[i] - minDepth[i];
            if (delta > maxDelta)
            {
                maxDelta = delta;
                maxDeltaNode = i;
            }
        }

        if (maxDeltaNode != -1)
        {
            var shortCircuitEdgeStart = cut.Parents(maxDeltaNode).OrderBy(parent => maxDepth[parent]).First();
            return SeverEdge(cut, shortCircuitEdgeStart, maxDeltaNode);
        }

        // If there's no depth discrepancies, then just sever one of the links to the first node with two parents
        foreach (var i in cut.GetDfsPostOrder())
        {
            if (i == exitNode)
                continue;

            var parents = cut.Parents(i);
            if (parents.Length <= 1)
                continue;

            return SeverEdge(cut, parents[0], i);
        }

        throw new ControlFlowGraphException("Cannot reduce SESE region", cut);
    }

    static ControlFlowGraph SeverEdge(ControlFlowGraph graph, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var labelName = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        var gotoNode = UAEmit.Goto(labelName);
        var label = graph.GetEdgeLabel(start, end);

        graph = graph
            .RemoveEdge(start, end)
            .AddNode(gotoNode, out var sourceNodeIndex)
            .AddEdge(start, sourceNodeIndex, label)
            .AddEdge(sourceNodeIndex, graph.ExitIndex, CfgEdge.True);

        if (labelName != null)
        {
            graph = graph.ReplaceNode(end,
                new ControlFlowGraph(0, 1,
                    new[] { UAEmit.Label(labelName), graph.Nodes[end] },
                    new[] { (0, 1, CfgEdge.True) }));
        }

        return graph;
    }
}
