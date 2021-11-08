using System.Linq;

namespace UAlbion.Scripting.Ast
{
    public sealed record Sequence(ICfgNode[] Statements) : ICfgNode
    {
        public Sequence(ICfgNode statement) : this(new[] { statement }) { }
        public override string ToString() => $"Seq({string.Join(", ", Statements.Select(x => x.ToString()))})";
        public void Accept(IAstVisitor visitor) => visitor.Visit(this);
        public override int GetHashCode() => Statements != null ? Statements.GetHashCode() : 0;
        public bool Equals(Sequence other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(Statements, other.Statements)) return true;
            return Statements != null && other.Statements != null && Statements.SequenceEqual(other.Statements);
        }
    }
}