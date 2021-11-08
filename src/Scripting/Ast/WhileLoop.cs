namespace UAlbion.Scripting.Ast
{
    public record WhileLoop(ICfgNode Condition, ICfgNode Body) : ICfgNode
    {
        public override string ToString() => $"While({Condition}, {Body})";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}