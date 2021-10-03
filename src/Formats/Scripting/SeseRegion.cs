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
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
            => Contents.Head.ToPseudocode(sb, indent, numeric);
    }
}