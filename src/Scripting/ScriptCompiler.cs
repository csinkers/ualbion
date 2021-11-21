using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class ScriptCompiler
    {
        public static EventLayout Compile(string source)
        {
            if (!ScriptParser.TryParse(source, out var ast, out var error, out _))
                throw new InvalidOperationException(error);

            var compiled = ExpandAstToGraphs(ast);
            return EventLayout.Build(compiled);
        }

        public static List<ControlFlowGraph> ExpandAstToGraphs(ICfgNode ast)
        {
            throw new NotImplementedException();
        }
    }
}