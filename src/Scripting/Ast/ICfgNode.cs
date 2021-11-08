namespace UAlbion.Scripting.Ast
{
    public interface ICfgNode
    {
        void Accept(IAstVisitor visitor);
    }
}
