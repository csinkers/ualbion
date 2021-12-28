namespace UAlbion.Scripting.Ast;

public record DoLoop(ICfgNode Condition, ICfgNode Body) : ICfgNode
{
    public override string ToString() => $"Do({Condition}, {Body})";
    public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public int Priority => int.MaxValue;
}