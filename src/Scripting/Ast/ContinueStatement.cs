namespace UAlbion.Scripting.Ast;

public record ContinueStatement : ICfgNode
{
    public override string ToString() => "Continue";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;
}