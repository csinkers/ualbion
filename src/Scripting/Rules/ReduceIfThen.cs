using System;

namespace UAlbion.Scripting.Rules
{
    public static class ReduceIfThen 
    {
        const string Description = "Reduce if-then";
        public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            foreach (var head in graph.GetDfsPostOrder())
            {
                var children = graph.Children(head);
                if (children.Length != 2)
                    continue;

                int after = -1;
                var then = -1;
                // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("after", after).AddPointer("then", then).ToString(); // For VS Code debug visualisation

                var parents0 = graph.Parents(children[0]);
                var parents1 = graph.Parents(children[1]);
                if (parents0.Length == 1)
                {
                    var grandChildren = graph.Children(children[0]);
                    if (grandChildren.Length == 1 && grandChildren[0] == children[1])
                    {
                        then = children[0];
                        after = children[1];
                    }
                }
                else if (parents1.Length == 1)
                {
                    var grandChildren = graph.Children(children[1]);
                    if (grandChildren.Length == 1 && grandChildren[0] == children[0])
                    {
                        then = children[1];
                        after = children[0];
                    }
                }

                if (after == -1 || then == -1 || after == head)
                    continue;

                var label = graph.GetEdgeLabel(head, then);
                var condition = label == CfgEdge.False ? Emit.Negation(graph.Nodes[head]) : graph.Nodes[head];

                var newNode = Emit.If(condition, graph.Nodes[then]);
                return (graph.RemoveNode(then).ReplaceNode(head, newNode), Description);
            }

            return (graph, null);
        }
    }
}