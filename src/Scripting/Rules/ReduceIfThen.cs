using System;

namespace UAlbion.Scripting.Rules;

public static class ReduceIfThen 
{
    const string Description = "Reduce if-then";
    public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        foreach (var head in graph.GetDfsPostOrder())
        {
            var (trueChild, falseChild) = graph.GetBinaryChildren(head);
            if (!trueChild.HasValue || !falseChild.HasValue)
                continue;

            int after = -1;
            var then = -1;
            // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("after", after).AddPointer("then", then).ToString(); // For VS Code debug visualisation

            var parents0 = graph.Parents(trueChild.Value);
            var parents1 = graph.Parents(falseChild.Value);
            if (parents0.Length == 1)
            {
                var grandChildren = graph.Children(trueChild.Value);
                if (grandChildren.Length == 1 && grandChildren[0] == falseChild.Value)
                {
                    then = trueChild.Value;
                    after = falseChild.Value;
                }
            }
            else if (parents1.Length == 1)
            {
                var grandChildren = graph.Children(falseChild.Value);
                if (grandChildren.Length == 1 && grandChildren[0] == trueChild.Value)
                {
                    then = falseChild.Value;
                    after = trueChild.Value;
                }
            }

            if (after == -1 || then == -1 || after == head)
                continue;

            var condition = then == falseChild.Value ? Emit.Negation(graph.Nodes[head]) : graph.Nodes[head];
            var newNode = Emit.If(condition, graph.Nodes[then]);
            return (graph.RemoveNode(then).ReplaceNode(head, newNode), Description);
        }

        return (graph, null);
    }
}