namespace UAlbion.Scripting.Ast
{
    public record BreakStatement : ICfgNode
    {
        public override string ToString() => "Break";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
        public int Priority => int.MaxValue;
    }
}