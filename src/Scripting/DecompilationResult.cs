using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class DecompilationResult
{
    public DecompilationResult()
        : this(string.Empty,
            [],
            [],
            new Dictionary<int, int>(),
            [])
    {
    }

    public DecompilationResult(string script,
        ScriptPart[] parts,
        EventRegion[] eventRegions,
        IReadOnlyDictionary<int, int> eventRegionLookup,
        ICfgNode[] nodes)
    {
        Script = script;
        Parts = parts ?? throw new ArgumentNullException(nameof(parts));
        EventRegions = eventRegions ?? throw new ArgumentNullException(nameof(eventRegions));
        EventRegionLookup = eventRegionLookup ?? throw new ArgumentNullException(nameof(eventRegionLookup));
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
    }

    public string Script { get; }
    public ScriptPart[] Parts { get; }
    public EventRegion[] EventRegions { get; }
    public IReadOnlyDictionary<int, int> EventRegionLookup { get; }
    public ICfgNode[] Nodes { get; }
    public override string ToString() => Script;
}