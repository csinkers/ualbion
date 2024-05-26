using System;

namespace UAlbion.Scripting.Rules;

public static class ReduceIfThenElse
{
    const string Description = "Reduce if-then-else";

    public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        foreach (var head in graph.GetDfsPostOrder())
        {
            var (trueChild, falseChild) = graph.GetBinaryChildren(head);
            if (!trueChild.HasValue || !falseChild.HasValue)
                continue;

            var left = trueChild.Value;
            var right = falseChild.Value;
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

            var after = isRegularIfThenElse ? leftChildren[0] : -1;

            if (after == head)
                continue;

            var newNode = UAEmit.IfElse(
                graph.Nodes[head],
                graph.Nodes[left],
                graph.Nodes[right]);

            var updated = graph;
            if (isRegularIfThenElse)
                updated = updated.AddEdge(head, after, CfgEdge.True);

            return (updated
                .RemoveNode(left)
                .RemoveNode(right)
                .ReplaceNode(head, newNode), Description);
        }

        return (graph, null);
    }
}