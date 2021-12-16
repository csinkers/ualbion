using System;
using System.Linq;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting.Rules
{
    public static class ReduceLoops
    {
        public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var loops = graph.GetLoops();
            foreach (var index in graph.GetDfsPostOrder())
            {
                foreach (var loop in loops)
                {
                    if (loop.IsMultiExit)
                    {
                        // Grow loop then reduce
                        continue;
                    }

                    foreach (var part in loop.Body)
                    {
                        // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
                        if (part.Index != index)
                            continue;

                        var updated = ReduceContinue(part, graph, index, loop);
                        if (updated != graph)
                            return (updated, "Reduce continue");

                        updated = ReduceBreak(part, graph, index, loop);
                        if (updated != graph)
                            return (updated, "Reduce break");
                    }

                    if (loop.Header.Index != index
                        || loop.IsMultiExit
                        || loop.Body.Count != 1
                        || loop.Body[0].OutsideEntry)
                    {
                        continue;
                    }

                    int tail = loop.Body[0].Index;
                    if (loop.Header.Break)
                    {
                        var updated = ReduceWhileLoop(graph, index, tail, loop.Header.Negated);
                        if (updated != graph)
                            return (updated, "Reduce while loop");
                    }
                    else if (loop.Body.All(x => !x.Break)) // Infinite while loop
                    {
                        var updated = ReduceInfiniteWhileLoop(graph, index, tail);
                        if (updated != graph)
                            return (updated, "Reduce infinite while loop");
                    }
                    else
                    {
                        var updated = ReduceDoLoop(graph, index, tail);
                        if (updated != graph)
                            return (updated, "Reduce do loop");
                    }
                }
            }
            return (graph, null);
        }

        static ControlFlowGraph ReduceContinue(LoopPart part, ControlFlowGraph graph, int index, CfgLoop loop)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
            if (part.OutsideEntry || part.Tail || part.Header || !part.Continue || part.Break)
                return graph;

            var children = graph.Children(index);
            var continueNode = Emit.Continue();
            if (children.Length == 1)
            {
                var seq = Emit.Seq(graph.Nodes[index], continueNode);
                return graph
                    .ReplaceNode(index, seq)
                    .RemoveEdge(index, loop.Header.Index);
            }

            if (children.Length == 2)
                return ReplaceLoopBranch(graph, index, loop.Header.Index, continueNode);

            throw new ControlFlowGraphException($"Continue at {index} has unexpected child count ({children.Length})", graph);
        }

        static ControlFlowGraph ReduceBreak(LoopPart part, ControlFlowGraph graph, int index, CfgLoop loop)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
            if (part.OutsideEntry || part.Tail || part.Header || part.Continue || !part.Break)
                return graph;

            var children = graph.Children(index);

            var breakNode = Emit.Break();
            if (children.Length != 2)
                throw new ControlFlowGraphException($"Break at {index} has unexpected child count ({children.Length})", graph);

            bool isfirstChildInLoop = loop.Body.Any(x => x.Index == children[0]);
            var target = isfirstChildInLoop ? children[1] : children[0];

            if (target != loop.MainExit)
            {
                var targetChildren = graph.Children(target);
                if (targetChildren.Length != 1 || targetChildren[0] != loop.MainExit)
                    return graph;

                var condition = graph.Nodes[index];
                if (graph.GetEdgeLabel(index, target) == CfgEdge.False)
                    condition = Emit.Negation(condition);

                var newBlock = Emit.Seq(graph.Nodes[target], Emit.Break());
                var ifNode = Emit.If(condition, newBlock);
                return graph
                    .RemoveNode(target)
                    .ReplaceNode(index, ifNode);
            }

            return ReplaceLoopBranch(graph, index, target, breakNode);
        }

        static ControlFlowGraph ReplaceLoopBranch(ControlFlowGraph graph, int index, int target, ICfgNode newNode)
        {
            var label = graph.GetEdgeLabel(index, target);
            var condition = graph.Nodes[index];

            if (label == CfgEdge.False)
                condition = Emit.Negation(condition);

            var ifNode = Emit.If(condition, newNode);
            return graph
                .ReplaceNode(index, ifNode)
                .RemoveEdge(index, target);
        }

        static ControlFlowGraph ReduceInfiniteWhileLoop(ControlFlowGraph graph, int head, int tail)
        {
            var exitNode = graph.GetFirstEmptyExitNode();
            var label = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var subgraph = new ControlFlowGraph(new[]
                {
                    Emit.Empty(), // 0
                    Emit.Label(label), // 1
                    graph.Nodes[head], // 2
                    graph.Nodes[tail], // 3
                    Emit.Goto(label), // 4
                    Emit.Empty(), // 5
                },
                new[] { (0, 1, CfgEdge.True), (1, 2, CfgEdge.True), (2, 3, CfgEdge.True), (3, 4, CfgEdge.True), (4, 5, CfgEdge.True), });

            return graph
                .RemoveNode(tail)
                .AddEdge(head, exitNode, CfgEdge.True)
                .ReplaceNode(head, subgraph);
        }

        static ControlFlowGraph ReduceWhileLoop(ControlFlowGraph graph, int head, int tail, bool negated)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("tail", tail).ToString(); // For VS Code debug visualisation
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 1)
            {
                bool isfirstChildInLoop = head == children[0];
                var target = isfirstChildInLoop ? children[1] : children[0];
                return ReplaceLoopBranch(graph, tail, target, Emit.Break());
            }

            var condition = graph.Nodes[head];
            if (negated)
                condition = Emit.Negation(condition);

            var newNode = Emit.While(condition, graph.Nodes[tail]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }

        static ControlFlowGraph ReduceDoLoop(ControlFlowGraph graph, int head, int tail)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("head", head).AddPointer("tail", tail).ToString(); // For VS Code debug visualisation
            var updated = graph;
            var children = graph.Children(tail);
            if (children.Length != 2)
                return updated;

            foreach (int child in children)
                if (child != head)
                    updated = updated.AddEdge(head, child, CfgEdge.True);

            var condition = graph.Nodes[tail];
            if (graph.GetEdgeLabel(tail, head) == CfgEdge.False)
                condition = Emit.Negation(condition);

            var newNode = Emit.Do(condition, graph.Nodes[head]);
            return updated
                .RemoveNode(tail)
                .ReplaceNode(head, newNode);
        }
    }
}