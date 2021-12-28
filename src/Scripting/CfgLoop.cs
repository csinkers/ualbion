using System.Collections.Immutable;

namespace UAlbion.Scripting;

public class CfgLoop
{
    public LoopPart Header { get; }
    public ImmutableList<LoopPart> Body { get; }
    public ImmutableList<int> Exits { get; }
    public bool IsMultiExit => Exits.Count > 1;
    public int? MainExit { get; }

    public CfgLoop(LoopPart header, ImmutableList<LoopPart> body, ImmutableList<int> exits, int? mainExit)
    {
        Header = header;
        Body = body;
        Exits = exits;
        MainExit = mainExit;
    }
}