using System;
using System.Linq;

namespace UAlbion.Scripting.Ast;

public sealed record Sequence : ICfgNode
{
    public ICfgNode[] Statements { get; init; }
    public Sequence(ICfgNode[] statements)
    {
        Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        if(statements.Length < 2)
            throw new ArgumentException("Tried to create sequence with less than 2 statements");
#if DEBUG
        if (statements.OfType<Sequence>().Any())
            throw new ArgumentException("Tried to create nested sequence");
#endif
    }
    public void Deconstruct(out ICfgNode[] statements) => statements = Statements;

    public override string ToString() => $"Seq({string.Join(", ", Statements.Select(x => x.ToString()))})";
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public override int GetHashCode() => Statements != null ? Statements.GetHashCode() : 0;
    public bool Equals(Sequence other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (ReferenceEquals(Statements, other.Statements)) return true;
        return Statements != null && other.Statements != null && Statements.SequenceEqual(other.Statements);
    }
    public int Priority => int.MaxValue;
}