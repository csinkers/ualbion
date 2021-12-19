namespace UAlbion.Scripting.Rules
{
    public static class LoopConverter
    {
        public static (ControlFlowGraph result, string description) Apply(ControlFlowGraph graph)
        {
            // TODO: Analyse loops, checking for ifs with a break at the start of end. Convert to while/do as appropriate.
            return (graph, "Convert loops");
        }
    }
}