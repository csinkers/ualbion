using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting.Rules
{
    public static class RemoveEmptyNodes
    {
        public static (ControlFlowGraph result, string description) Apply(ControlFlowGraph graph) 
            => (graph.AcceptBuilder(new EmptyNodeRemovalVisitor()), "Remove empty nodes");

        class EmptyNodeRemovalVisitor : BaseAstBuilderVisitor
        {
            protected override ICfgNode Build(Sequence sequence)
            {
                var result = new List<ICfgNode>();
                bool trivial = true;
                foreach (var node in sequence.Statements)
                {
                    if (node is EmptyNode)
                    {
                        trivial = false;
                        continue;
                    }

                    node.Accept(this);
                    if (Result != null)
                        trivial = false;

                    if (Result is Sequence seq)
                    {
                        foreach (var statement in seq.Statements)
                            result.Add(statement);
                    }
                    else result.Add(Result ?? node);
                }

                if (trivial)
                    return null;

                return result.Count switch
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
}