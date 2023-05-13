namespace UAlbion.Scripting.Ast;

public record ControlFlowNode(ControlFlowGraph Graph) : ICfgNode
{
    public override string ToString() => "SubGraph";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;
}