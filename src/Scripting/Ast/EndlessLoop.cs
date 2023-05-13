namespace UAlbion.Scripting.Ast;

public record EndlessLoop(ICfgNode Body) : ICfgNode
{
    public override string ToString() => $"Loop({Body})";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;
}