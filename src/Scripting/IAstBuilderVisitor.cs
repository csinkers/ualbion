using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public interface IAstBuilderVisitor : IAstVisitor
    {
        ICfgNode Result { get; }
    }
}