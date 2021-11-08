using System;
using System.Collections.Immutable;

namespace UAlbion.Scripting.Ast
{
    public class SeseRegion : ICfgNode
    {
        public SeseRegion(ControlFlowGraph contents)
        {
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
            DecisionNodes = contents.GetBranchNodes().ToImmutableArray();
            CodeNodes = contents.GetNonBranchNodes().ToImmutableArray();
        }

        public ControlFlowGraph Contents { get; }
        public ImmutableArray<int> DecisionNodes { get; }
        public ImmutableArray<int> CodeNodes { get; }
        public override string ToString() => "SESE";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
        public int Priority => int.MaxValue;
    }
}