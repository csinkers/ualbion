using System;
using System.Collections.Generic;
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

                    if (loop.Header.Index != index) // Handle non-header exits and back edges
                    {
                        foreach (var part in loop.Body)
                        {
                            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation
                            if (part.Index != index)
                                continue;

                            var updated = ReduceContinue(graph, loop, part);
                            if (updated != graph)
                                return (updated, "Reduce continue");

                            updated = ReduceBreak(graph, loop, part);
                            if (updated != graph)
                                return (updated, "Reduce break");
                        }
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
                    else if (loop.Body[0].Break)
                    {
                        var updated = ReduceDoLoop(graph, index, tail);
                        if (updated != graph)
                            return (updated, "Reduce do loop");
                    }
                    else
                    {
                        var updated = ReduceGenericLoop(graph, index, tail);
                        if (updated != graph)
                            return (updated, "Reduce generic loop");
                    }
                }
            }
            return (graph, null);
        }

        static ControlFlowGraph ReduceContinue(ControlFlowGraph graph, CfgLoop loop, LoopPart part)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("index", index).AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation

            // Outside entry = non-structured code, so give up
            // Tail = the tail of a loop always continues by default, don't need a statement - the top-level loop reduction will handle it.
            // Header = continuing header should be handled by simple loop rule
            // !Continue = if it's not a jump back to the header, it's not a continue
            // Break = shouldn't be both a continue and a break
            if (part.OutsideEntry || part.Tail || part.Header || !part.Continue || part.Break)
                return graph;

            var children = graph.Children(part.Index);
            switch (children.Length)
            {
                case 1:
                {
                    var seq = Emit.Seq(graph.Nodes[part.Index], Emit.Continue());
                    var tailIndex = loop.Body.First(x => x.Tail).Index;
                    return graph
                        .ReplaceNode(part.Index, seq)
                        .RemoveEdge(part.Index, loop.Header.Index)
                        .AddEdge(part.Index, tailIndex, CfgEdge.True);
                }

                case 2:
                    return ReplaceLoopBranch(graph, part.Index, loop.Header.Index, Emit.Continue());

                default:
                    throw new ControlFlowGraphException($"Continue at {part.Index} has unexpected child count ({children.Length})", graph);
            }
        }

        static ControlFlowGraph ReduceBreak(ControlFlowGraph graph, CfgLoop loop, LoopPart part)
        {
            // Func<string> vis = () => graph.ToVis().AddPointer("part", part.Index).ToString(); // For VS Code debug visualisation

            // Break = needs to exit the loop to be a break
            if (!part.Break)
                return graph;

            // Outside entry = non-structured code, so give up
            if (part.OutsideEntry)
                return graph;

            if (part.Header) // Header-breaks will be structured into a while loop
                return graph;

            if (part.Tail && !loop.Header.Break) // Tail-breaks will be structured into a do loop as long as the header isn't also a break (while loop is preferred)
                return graph;

            var children = graph.Children(part.Index);
             
            if (children.Length != 2)
                throw new ControlFlowGraphException($"Break at {part.Index} has unexpected child count ({children.Length})", graph);

            bool isfirstChildInLoop = loop.Body.Any(x => x.Index == children[0]) || children[0] == loop.Header.Index;
            var exitTarget = isfirstChildInLoop ? children[1] : children[0];

            // Add LoopSuccessor edge if this is the last link to the MainExit.
            var remainingLoopChildren = loop.Body
                .Where(x => x.Index != part.Index)
                .Aggregate(
                    (IEnumerable<int>)graph.Children(loop.Header.Index),
                    (current, x) => current.Union(graph.Children(x.Index)));

            if (loop.MainExit.HasValue && !remainingLoopChildren.Contains(loop.MainExit.Value))
                graph = graph.AddEdge(loop.Header.Index, loop.MainExit.Value, CfgEdge.LoopSuccessor);

            if (exitTarget != loop.MainExit)
            {
                var targetChildren = graph.Children(exitTarget);
                if (targetChildren.Length != 1 || targetChildren[0] != loop.MainExit)
                    return graph;

                var condition = graph.Nodes[part.Index];
                if (graph.GetEdgeLabel(part.Index, exitTarget) == CfgEdge.False)
                    condition = Emit.Negation(condition);

                var ifNode = Emit.If(condition, Emit.Seq(graph.Nodes[exitTarget], Emit.Break()));
                return graph
                    .RemoveNode(exitTarget)
                    .ReplaceNode(part.Index, ifNode);
            }

            return ReplaceLoopBranch(graph, part.Index, exitTarget, Emit.Break());
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

        static ControlFlowGraph ReduceGenericLoop(ControlFlowGraph graph, int head, int tail)
        {
            var loopNode = Emit.Loop(Emit.Seq(graph.Nodes[head], graph.Nodes[tail]));
            return graph
                .RemoveNode(tail)
                .ReplaceNode(head, loopNode);
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