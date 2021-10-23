using System;
using System.Collections.Generic;

namespace UAlbion.Scripting
{
    public class CfgCutResult
    {
        public CfgCutResult(
            ControlFlowGraph cut,
            ControlFlowGraph remainder,
            List<(int start, int end, bool label)> cutToRemainderEdges,
            List<(int start, int end, bool label)> remainderToCutEdges)
        {
            Cut = cut ?? throw new ArgumentNullException(nameof(cut));
            Remainder = remainder ?? throw new ArgumentNullException(nameof(remainder));
            CutToRemainderEdges = cutToRemainderEdges ?? throw new ArgumentNullException(nameof(cutToRemainderEdges));
            RemainderToCutEdges = remainderToCutEdges ?? throw new ArgumentNullException(nameof(remainderToCutEdges));
        }
        public ControlFlowGraph Cut { get; }
        public ControlFlowGraph Remainder { get; }
        public List<(int start, int end, bool label)> CutToRemainderEdges { get; }
        public List<(int start, int end, bool label)> RemainderToCutEdges { get; }
    }
}
