using System;
using System.Linq;

namespace UAlbion.Scripting.Rules;

public static class ReduceSimpleWhile 
{
    public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        var simpleLoopIndices =
            from index in graph.GetDfsPostOrder()
            let children = graph.Children(index)
            where children.Length == 2 && children.Contains(index)
            select index;

        foreach (var index in simpleLoopIndices)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).ToString(); // For VS Code debug visualisation

            // Add an empty header node, which will put it into the form expected by the general-case loop reducer
            int? successor = null;
            foreach (var child in graph.Children(index).Where(x => graph.GetEdgeLabel(index, x) == CfgEdge.LoopSuccessor))
                successor = child;

            graph = graph.InsertBefore(index, Emit.Empty(), out var newHeaderIndex);

            if (successor.HasValue)
            {
                graph = graph
                    .RemoveEdge(index, successor.Value)
                    .AddEdge(newHeaderIndex, successor.Value, CfgEdge.LoopSuccessor);
            }

            return (graph, "Add empty header for single-node loop");
        }

        return (graph, null);
    }
}