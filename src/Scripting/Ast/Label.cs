namespace UAlbion.Scripting.Ast;

public record Label(string Name) : ICfgNode
{
    public override string ToString() => $"Label({Name})";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;
}