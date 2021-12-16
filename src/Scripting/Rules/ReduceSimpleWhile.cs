using System;
using System.Linq;

namespace UAlbion.Scripting.Rules
{
    public static class ReduceSimpleWhile 
    {
        const string Description = "Reduce simple while";
        public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var simpleLoopIndices =
                from index in graph.GetDfsPostOrder()
                let children = graph.Children(index)
                where children.Length is 1 or 2 && children.Contains(index)
                select index;

            foreach (var index in simpleLoopIndices)
            {
                // Func<string> vis = () => graph.ToVis().AddPointer("index", index).ToString(); // For VS Code debug visualisation
                if (graph.Children(index).Length == 1)
                    return ReduceEmptyInfiniteWhileLoop(graph, index);

                var condition = graph.Nodes[index];
                if (graph.GetEdgeLabel(index, index) == CfgEdge.False)
                    condition = Emit.Negation(condition);

                var updated = graph
                    .ReplaceNode(index, Emit.While(condition, null))
                    .RemoveEdge(index, index);

                return (updated, Description);
            }

            return (graph, null);
        }

        static (ControlFlowGraph, string) ReduceEmptyInfiniteWhileLoop(ControlFlowGraph graph, int index)
        {
            var exitNode = graph.GetFirstEmptyExitNode();
            var label = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var subgraph = new ControlFlowGraph(new[]
                {
                    Emit.Empty(), // 0
                    Emit.Label(label), // 1
                    graph.Nodes[index], // 2
                    Emit.Goto(label), // 3
                    Emit.Empty(), // 4
                },
                new[] { (0,1,CfgEdge.True), (1,2,CfgEdge.True), (2,3,CfgEdge.True), (3,4,CfgEdge.True), });

            return (graph.AddEdge(index, exitNode, CfgEdge.True).ReplaceNode(index, subgraph), "Reduce empty infinite while");
        }
    }
}