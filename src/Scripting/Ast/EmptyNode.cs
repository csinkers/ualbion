namespace UAlbion.Scripting.Ast;

public record EmptyNode : ICfgNode // Used for empty entry / exit nodes
{
    public override string ToString() => "ø";
    public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public int Priority => int.MaxValue;
}