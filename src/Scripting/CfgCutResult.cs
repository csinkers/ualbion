using System;
using System.Collections.Generic;

namespace UAlbion.Scripting;

public class CfgCutResult
{
    public CfgCutResult(
        ControlFlowGraph cut,
        ControlFlowGraph remainder,
        List<(int remainderIndex, CfgEdge label)> remainderToCutEdges,
        List<(int remainderIndex, CfgEdge label)> cutToRemainderEdges)
    {
        Cut = cut ?? throw new ArgumentNullException(nameof(cut));
        Remainder = remainder ?? throw new ArgumentNullException(nameof(remainder));
        CutToRemainderEdges = cutToRemainderEdges ?? throw new ArgumentNullException(nameof(cutToRemainderEdges));
        RemainderToCutEdges = remainderToCutEdges ?? throw new ArgumentNullException(nameof(remainderToCutEdges));
    }
    public ControlFlowGraph Cut { get; }
    public ControlFlowGraph Remainder { get; }
    public List<(int remainderIndex, CfgEdge label)> RemainderToCutEdges { get; }
    public List<(int remainderIndex, CfgEdge label)> CutToRemainderEdges { get; }

    public ControlFlowGraph Merge(ControlFlowGraph restructured)
    {
        ArgumentNullException.ThrowIfNull(restructured);

        var (updated, mapping) = Remainder.Merge(restructured);

        if (RemainderToCutEdges.Count > 0)
        {
            foreach (var (start, label) in RemainderToCutEdges)
                updated = updated.AddEdge(start, mapping[restructured.EntryIndex], label);
        }
        else updated = updated.SetEntry(mapping[restructured.EntryIndex]);

        if (CutToRemainderEdges.Count > 0)
        {
            foreach (var (end, label) in CutToRemainderEdges)
                updated = updated.AddEdge(mapping[restructured.ExitIndex], end, label);
        }
        else updated = updated.SetExit(mapping[restructured.ExitIndex]);

        return updated;
    }
}