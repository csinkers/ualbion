using System.Collections.Generic;
using UAlbion.Scripting;

namespace UAlbion.Formats
{
    public static class AlbionCompiler
    {
        public static EventLayout Compile(string script, List<(string, IGraph)> steps = null)
            => ScriptCompiler.Compile(script, null, steps);
    }
}
