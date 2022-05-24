using System.Collections.Generic;
using UAlbion.Formats.Ids;
using UAlbion.Scripting;

namespace UAlbion.Formats
{
    public static class AlbionCompiler
    {
        public static EventLayout Compile(string script, TextId stringContext, List<(string, IGraph)> steps = null)
            => ScriptCompiler.Compile(script, e => AlbionEventTransforms.TransformForCompiler(e, stringContext), steps);
    }
}
