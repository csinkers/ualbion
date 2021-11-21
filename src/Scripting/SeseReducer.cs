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

            if (maxDeltaNode != -1)
            {
                var shortCircuitEdgeStart = cut.Parents(maxDeltaNode).OrderBy(parent => maxDepth[parent]).First();
                return SeverEdge(cut, shortCircuitEdgeStart, maxDeltaNode, exitNode);
            }

            // If there's no depth discrepancies, then just sever one of the links to the first node with two parents
            foreach (var i in cut.GetDfsPostOrder())
            {
                if (i == exitNode)
                    continue;

                var parents = cut.Parents(i);
                if (parents.Length <= 1)
                    continue;

                return SeverEdge(cut, parents[0], i, exitNode);
            }

            throw new ControlFlowGraphException("Cannot reduce SESE region", cut);
        }

        static ControlFlowGraph SeverEdge(ControlFlowGraph cut, int start, int end, int exitNode)
        {
            var shortCircuitEdgeLabel = cut.GetEdgeLabel(start, end);
            var result = cut.RemoveEdge(start, end);

            var labelName = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var gotoNode = Emit.Goto(labelName);
            var label = Emit.Label(labelName);

            result = result.AddNode(gotoNode, out var gotoIndex);
            result = result.AddNode(label, out var labelIndex);
            result = result.AddEdge(start, gotoIndex, shortCircuitEdgeLabel);
            result = result.AddEdge(gotoIndex, exitNode, true); // Dummy edge to ensure exit node stays unique

            var parents = result.Parents(end);
            foreach (var parent in parents)
            {
                var edgeLabel = cut.GetEdgeLabel(parent, end);
                result = result.RemoveEdge(parent, end).AddEdge(parent, labelIndex, edgeLabel);
            }

            result = result.AddEdge(labelIndex, end, true);

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