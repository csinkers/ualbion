using System.Collections.Generic;

namespace UAlbion.Formats.Scripting
{
    public class CfgLoop
    {
        public LoopPart Header { get; }
        public IList<LoopPart> Body { get; }
        public bool IsMultiExit { get; }
        public int? MainExit { get; }

        public CfgLoop(LoopPart header, IList<LoopPart> body, bool isMultiExit, int? mainExit)
        {
            Header = header;
            Body = body;
            IsMultiExit = isMultiExit;
            MainExit = mainExit;
        }
    }
}