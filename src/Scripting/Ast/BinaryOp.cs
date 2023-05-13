namespace UAlbion.Scripting.Ast;

public record BinaryOp(ScriptOp Operation, ICfgNode Left, ICfgNode Right) : ICfgNode
{
    public override string ToString() => $"{Operation}({Left}, {Right})";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => Operation.Priority();
}