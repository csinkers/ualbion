using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting.Rules;

public static class RemoveEmptyNodes
{
    public static (ControlFlowGraph result, string description) Apply(ControlFlowGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        return (graph.AcceptBuilder(new EmptyNodeRemovalVisitor()), "Remove empty nodes");
    }

    sealed class EmptyNodeRemovalVisitor : BaseAstBuilderVisitor
    {
        protected override ICfgNode Build(Sequence sequence)
        {
            List<ICfgNode> result = null;
            for (var index = 0; index < sequence.Statements.Length; index++)
            {
                var node = sequence.Statements[index];
                if (node is EmptyNode)
                {
                    if (result == null)
                    {
                        result = new List<ICfgNode>();
                        for (int i = 0; i < index; i++)
                            result.Add(sequence.Statements[i]);
                    }

                    continue;
                }

                node.Accept(this);
                if (result == null)
                {
                    if (Result == null)
                        continue;

                    result = new List<ICfgNode>();
                    for (int i = 0; i < index; i++)
                        result.Add(sequence.Statements[i]);
                }

                if (Result is Sequence seq)
                {
                    foreach (var statement in seq.Statements)
                        result.Add(statement);
                }
                else result.Add(Result ?? node);
            }

            return result == null ? null : result.Count switch
            {
                // If it was a sequence of all empty nodes, emit a single empty node
                // so the next iteration will take care of it
                0 => Emit.Empty(),
                1 => result[0],
                _ => Emit.Seq(result.ToArray())
            };
        }
    }
}