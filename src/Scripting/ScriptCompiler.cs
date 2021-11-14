using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class ScriptCompiler
    {
        public static (List<IEventNode> events, List<int> chains) Compile(string source)
        {
            if (!ScriptParser.TryParse(source, out var ast, out var error, out _))
                throw new InvalidOperationException(error);

            var compiled = ExpandAstToGraph(ast);
            return LayoutGraph(compiled);
        }

        public static ControlFlowGraph ExpandAstToGraph(ICfgNode ast)
        {
            throw new NotImplementedException();
        }

        public static (List<IEventNode> events, List<int> chains) LayoutGraph(ControlFlowGraph compiled)
        {
            throw new NotImplementedException();
        }
    }
}