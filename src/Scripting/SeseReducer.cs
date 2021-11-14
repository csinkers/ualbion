using System;
using System.Linq;

namespace UAlbion.Scripting
{
    public static class SeseReducer
    {
        public static ControlFlowGraph Reduce(ControlFlowGraph cut)
        {
            var minDepth = FindShortestPaths(cut);
            var maxDepth = FindLongestPaths(cut);
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

            if (maxDeltaNode == -1)
                return cut;

            var shortCircuitEdgeStart = cut.Parents(maxDeltaNode).OrderBy(parent => maxDepth[parent]).First();
            var shortCircuitEdgeLabel = cut.GetEdgeLabel(shortCircuitEdgeStart, maxDeltaNode);
            var result = cut.RemoveEdge(shortCircuitEdgeStart, maxDeltaNode);

            var labelName = $"L_{Guid.NewGuid():N}";
            var gotoNode = Emit.Goto(labelName);
            var label = Emit.Label(labelName);

            result = result.AddNode(gotoNode, out var gotoIndex);
            result = result.AddNode(label, out var labelIndex);
            result = result.AddEdge(shortCircuitEdgeStart, gotoIndex, shortCircuitEdgeLabel);
            result = result.AddEdge(gotoIndex, exitNode, true); // Dummy edge to ensure exit node stays unique

            var parents = result.Parents(maxDeltaNode);
            foreach (var parent in parents)
            {
                var edgeLabel = cut.GetEdgeLabel(parent, maxDeltaNode);
                result = result.RemoveEdge(parent, maxDeltaNode).AddEdge(parent, labelIndex, edgeLabel);
            }

            result = result.AddEdge(labelIndex, maxDeltaNode, true);

            return result;
        }

        public static int[] FindShortestPaths(ControlFlowGraph graph)
        {
            var result = new int[graph.Nodes.Count];
            Array.Fill(result, int.MaxValue);
            result[graph.HeadIndex] = 0;

            foreach (var i in graph.GetTopogicalOrder())
                foreach (var child in graph.Children(i))
                    if (result[child] > result[i] + 1)
                        result[child] = result[i] + 1;

            return result;
        }

        public static int[] FindLongestPaths(ControlFlowGraph graph)
        {
            var result = new int[graph.Nodes.Count];
            Array.Fill(result, int.MinValue);
            result[graph.HeadIndex] = 0;

            foreach (var i in graph.GetTopogicalOrder())
                foreach (var child in graph.Children(i))
                    if (result[child] < result[i] + 1)
                        result[child] = result[i] + 1;

            return result;
        }
    }
}