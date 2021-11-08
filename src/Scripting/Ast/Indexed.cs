namespace UAlbion.Scripting.Ast
{
    public record Indexed(ICfgNode Parent, ICfgNode Index) : ICfgNode
    {
        public override string ToString() => $"Indexed({Parent}, {Index})";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}