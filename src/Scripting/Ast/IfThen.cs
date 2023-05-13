namespace UAlbion.Scripting.Ast;

public record IfThen(ICfgNode Condition, ICfgNode Body) : ICfgNode
{
    public override string ToString() => $"If({Condition}, {Body})";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;
}