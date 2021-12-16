using System;
using System.Linq;

namespace UAlbion.Scripting
{
    public static class SeseReducer
    {
        public static ControlFlowGraph Reduce(ControlFlowGraph cut)
        {
            var minDepth = cut.GetShortestPaths();
            var maxDepth = cut.GetLongestPaths();
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
            var labelName = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var gotoNode = Emit.Goto(labelName);
            return Decompiler.BreakEdge(cut, start, end, gotoNode, labelName);
        }
    }
}