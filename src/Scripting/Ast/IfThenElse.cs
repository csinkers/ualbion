namespace UAlbion.Scripting.Ast
{
    public record IfThenElse(ICfgNode Condition, ICfgNode TrueBody, ICfgNode FalseBody) : ICfgNode
    {
        public override string ToString() => $"IfElse({Condition}, {TrueBody}, {FalseBody})";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
        public int Priority => int.MaxValue;
    }
}