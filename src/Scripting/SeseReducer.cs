using System;
using System.Linq;

namespace UAlbion.Scripting
{
    public static class SeseReducer
    {
        public static ControlFlowGraph Reduce(ControlFlowGraph cut)
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
            var labelName = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var gotoNode = Emit.Goto(labelName);

            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (gotoNode == null) throw new ArgumentNullException(nameof(gotoNode));
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
                        new[] { Emit.Label(labelName), graph.Nodes[end] },
                        new[] { (0, 1, CfgEdge.True) }));
            }

            return graph;
        }
    }
}