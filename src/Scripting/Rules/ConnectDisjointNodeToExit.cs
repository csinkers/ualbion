using System;
using System.Linq;

namespace UAlbion.Scripting.Rules;

public static class ConnectDisjointNodeToExit 
{
    const string Description = "Connect disjoint node to exit";
    public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        var (reachability, reachableCount) = graph.Reverse().GetReachability(graph.ExitIndex);
        if (reachableCount == graph.ActiveNodeCount)
            return (graph, null);

        var acyclic = graph.RemoveBackEdges();
        var distances = acyclic.GetLongestPaths(acyclic.EntryIndex);
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

        if (winner == -1)
            return (graph, null);

        foreach (var backEdge in graph.GetBackEdges().Where(x => x.start == winner))
            return (graph.AddEdge(backEdge.end, graph.ExitIndex, CfgEdge.LoopSuccessor), Description);

        return (graph.AddEdge(winner, graph.ExitIndex, CfgEdge.DisjointGraphFixup), Description);
    }
}