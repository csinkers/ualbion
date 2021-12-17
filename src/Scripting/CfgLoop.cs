using System.Collections.Immutable;

namespace UAlbion.Scripting
{
    public class CfgLoop
    {
        public LoopPart Header { get; }
        public ImmutableList<LoopPart> Body { get; }
        public bool IsMultiExit { get; }
        public int? MainExit { get; }

        public CfgLoop(LoopPart header, ImmutableList<LoopPart> body, bool isMultiExit, int? mainExit)
        {
            Header = header;
            Body = body;
            IsMultiExit = isMultiExit;
            MainExit = mainExit;
        }
    }
}