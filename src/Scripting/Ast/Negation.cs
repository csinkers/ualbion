namespace UAlbion.Scripting.Ast;

public record Negation(ICfgNode Expression) : ICfgNode
{
    public override string ToString() => $"Negation({Expression})";
    public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public int Priority => 2;
}