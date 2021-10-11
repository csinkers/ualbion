using System;
using System.Collections.Immutable;
using System.Text;

namespace UAlbion.Formats.Scripting
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
        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
            => Contents.Head.ToPseudocode(sb, isStatement, numeric);
    }
}