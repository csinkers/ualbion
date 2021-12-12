using System;

namespace UAlbion.Scripting
{
    public static class LoopSimplifier
    {
        public static ControlFlowGraph SimplifyLoop(ControlFlowGraph graph, RecordFunc record)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            record("Simplify loop", graph);
            ControlFlowGraph previous = null;
            int iterations = 0;
            while (graph != previous)
            {
                iterations++;
                if (iterations > 1000)
                    throw new ControlFlowGraphException("Iteration overflow, assuming graph cannot be structured", graph);

                previous = graph;
                graph = record("Reduce sequence",     Decompiler.ReduceSequences(graph));   if (graph != previous) continue;
                graph = record("Reduce if-then",      Decompiler.ReduceIfThen(graph));      if (graph != previous) continue;
                graph = record("Reduce if-then-else", Decompiler.ReduceIfThenElse(graph));  if (graph != previous) continue;
                graph = record("Reduce loop",         ReduceLoop(graph));                   if (graph != previous) continue;
                graph = record("Reduce breaks",       ReduceBreak(graph, record));          if (graph != previous) continue;
                graph = record("Reduce continues",    ReduceContinue(graph, record));       if (graph != previous) continue;
                graph = record("Reduce back-edge",    ReduceBackEdge(graph));               if (graph != previous) continue;
                graph = record("Reduce SESE region",  Decompiler.ReduceSeseRegions(graph));
            }

            return graph;
        }

        static ControlFlowGraph ReduceBreak(ControlFlowGraph graph, RecordFunc record)
        {
            foreach (var start in graph.Parents(graph.ExitIndex))
                graph = record("Reduce break", Decompiler.BreakEdge(graph, start, graph.ExitIndex, Emit.Break(), null));

            return graph;
        }

        static ControlFlowGraph ReduceContinue(ControlFlowGraph graph, RecordFunc record)
        {
            var entryChildren = graph.Children(graph.EntryIndex);
            if (entryChildren.Length != 1)
                return graph;

            var header = entryChildren[0];
            foreach (var start in graph.Parents(header))
            {
                if (start == graph.EntryIndex)
                    continue;

                graph = record("Reduce continue", Decompiler.BreakEdge(graph, start, header, Emit.Continue(), null));
            }

            return graph;
        }

        static ControlFlowGraph ReduceLoop(ControlFlowGraph graph)
        {
            if (graph.ActiveNodeCount == 2)
            {
                graph = graph.ReplaceNode(graph.EntryIndex, Emit.Loop(graph.Nodes[graph.EntryIndex]));
                graph = graph.AddEdge(graph.EntryIndex, graph.ExitIndex, true);
            }

            return graph;
        }

        static ControlFlowGraph ReduceBackEdge(ControlFlowGraph graph)
        {
            return graph;
        }
    }
}