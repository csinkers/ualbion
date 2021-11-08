namespace UAlbion.Scripting.Ast
{
    public record Member(ICfgNode Parent, ICfgNode Child) : ICfgNode
    {
        public override string ToString() => $"Member({Parent}, {Child}";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}