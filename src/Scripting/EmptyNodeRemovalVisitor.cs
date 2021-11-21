using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public class EmptyNodeRemovalVisitor : BaseBuilderAstVisitor
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

            return result.Count == 1 
                ? result[0] 
                : Emit.Seq(result.ToArray());
        }
    }
}