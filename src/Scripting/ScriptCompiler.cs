using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class ScriptCompiler
    {
        public static EventLayout Compile(string source, List<(string, ControlFlowGraph)> steps = null)
        {
            RecordFunc record;
            if (steps != null)
            {
                var steps2 = steps;
                record = (description, graph) =>
                {
                    if (steps2.Count == 0 || steps[^1].Item2 != graph)
                        steps2.Add((description, graph));
                    return graph;
                };
            }
            else record = (_, x) => x;

            if (!ScriptParser.TryParse(source, out var ast, out var error, out _))
                throw new InvalidOperationException(error);

            var compiled = ExpandAstToGraphs(ast, record);
            return EventLayout.Build(compiled);
        }

        public static List<ControlFlowGraph> ExpandAstToGraphs(ICfgNode ast, RecordFunc record)
        {
            if (ast == null) throw new ArgumentNullException(nameof(ast));
            var start = Emit.Empty();
            var end = Emit.Empty();
            var graph = new ControlFlowGraph(new[] { start, ast, end }, new[] { (0, 1, true), (1, 2, true) });
            graph = ExpandGraph(graph, record);
            return SplitGraph(graph);
        }

        static List<ControlFlowGraph> SplitGraph(ControlFlowGraph graph) => new() { graph }; // TODO

        public static ControlFlowGraph ExpandGraph(ControlFlowGraph graph, RecordFunc record)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            ControlFlowGraph previous = null;
            graph = record("Begin compilation", graph);
            graph = record("Lower loops", LowerLoops(graph));

            while (graph != previous)
            {
                previous = graph;
                graph = record("Flatten CFG nodes",   FlattenCfgNodes(graph));  if (graph != previous) continue;
                graph = record("Expand sequence",     ExpandSequence(graph));   if (graph != previous) continue;
                graph = record("Expand if-then-else", ExpandIfThenElse(graph)); if (graph != previous) continue;
                graph = record("Expand if-then",      ExpandIfThen(graph));
            }

            graph = record("Resolve goto", ResolveGotos(graph));
            // TODO: Split graphs
            try
            {
                graph = record("Parse events", ParseEvents(graph));
            }
            catch {}

            graph = record("Remove empty nodes", RemoveEmptyNodes(graph));
            graph = record("Defragment", graph.Defragment());
            return graph;
        }

        static ControlFlowGraph RemoveEmptyNodes(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not EmptyNode)
                    continue;

                var children = graph.Children(index);
                if (children.Length != 1)
                    continue;

                var child = children[0];

                var parents = graph.Parents(index);
                if (parents.Length == 0) // Don't remove head node
                    continue;

                foreach (var parent in parents)
                    graph = graph.AddEdge(parent, child, graph.GetEdgeLabel(parent, index));
                graph = graph.RemoveNode(index);
            }

            return graph;
        }

        static ControlFlowGraph ParseEvents(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                var visitor = new EventParsingVisitor();
                try
                {
                    node.Accept(visitor);
                    if (visitor.Result != null)
                        graph = graph.ReplaceNode(index, visitor.Result);
                }
                catch (Exception ex) { throw new ControlFlowGraphException(ex.Message, graph); }
            }

            return graph;
        }

        static ControlFlowGraph FlattenCfgNodes(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not ControlFlowNode cfgNode)
                    continue;

                return graph.ReplaceNode(index, cfgNode.Graph);
            }

            return graph;
        }

        static ControlFlowGraph LowerLoops(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                var visitor = new LoopLoweringVisitor();
                node.Accept(visitor);
                var result = (ControlFlowNode)visitor.Result;
                if (result != null)
                    graph = graph.ReplaceNode(index, result);
            }
            return graph;
        }

        public static ControlFlowGraph ExpandSequence(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not Sequence seq)
                    continue;

                if (seq.Statements.Length < 2)
                    throw new ControlFlowGraphException($"Sequence {index} contained less than 2 statements!", graph);

                var cfg = new ControlFlowGraph(
                    seq.Statements,
                    Enumerable.Range(0, seq.Statements.Length - 1).Select(x => (x, x + 1, true)));

                return graph.ReplaceNode(index, cfg);
            }

            return graph;
        }

        public static ControlFlowGraph ExpandIfThenElse(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not IfThenElse ifElse)
                    continue;

                var cfg = new ControlFlowGraph(
                    new[]
                    {
                        ifElse.Condition, // 0
                        ifElse.TrueBody, // 1
                        ifElse.FalseBody, // 2
                        Emit.Empty() // 3
                    },
                    new []
                    {
                        (0,1,true),
                        (0,2,false),
                        (1,3,true),
                        (2,3,true),
                    });
                return graph.ReplaceNode(index, cfg);
            }

            return graph;
        }

        public static ControlFlowGraph ExpandIfThen(ControlFlowGraph graph)
        {
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not IfThen ifThen)
                    continue;

                bool negated = false;
                var condition = ifThen.Condition;
                if (ifThen.Condition is Negation negation)
                {
                    negated = true;
                    condition = negation.Expression;
                }

                var cfg = new ControlFlowGraph(
                    new[]
                    {
                        condition, // 0
                        ifThen.Body, // 1
                        Emit.Empty() // 2
                    },
                    new []
                    {
                        (0, 1, !negated),
                        (0, 2, negated),
                        (1, 2, true),
                    });
                return graph.ReplaceNode(index, cfg);
            }

            return graph;
        }

        public static ControlFlowGraph ResolveGotos(ControlFlowGraph graph)
        {
            // Build label name -> target dict
            var mapping = new Dictionary<string, int>();
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not Label label)
                    continue;

                mapping[label.Name] = index;
            }

            // Add edges and remove gotos
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not Goto statement)
                    continue;

                if (!mapping.TryGetValue(statement.Label, out var target))
                    throw new ControlFlowGraphException($"No label could be found for goto {index}: {statement.Label}", graph);

                var parents = graph.Parents(index);
                foreach (var parent in parents)
                    graph = graph.AddEdge(parent, target, graph.GetEdgeLabel(parent, index));

                graph = graph.RemoveNode(index);
            }

            // Remove labels
            foreach (var index in graph.GetDfsOrder())
            {
                var node = graph.Nodes[index];
                if (node is not Label label)
                    continue;

                if (!ScriptConstants.IsDummyLabel(label.Name))
                    continue;

                var parents = graph.Parents(index);
                var children = graph.Children(index);
                if (children.Length != 1)
                    throw new ControlFlowGraphException($"Label {index} ({label.Name}) did not have a single child", graph);

                var target = children[0];
                foreach (var parent in parents)
                    graph = graph.AddEdge(parent, target, graph.GetEdgeLabel(parent, index));

                graph = graph.RemoveNode(index);
            }

            return graph;
        }
    }
}
