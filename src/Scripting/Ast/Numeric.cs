namespace UAlbion.Scripting.Ast
{
    public record Numeric(int Value) : ICfgNode
    {
        public override string ToString() => $"{Value}";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
        public int Priority => 0;
    }
}