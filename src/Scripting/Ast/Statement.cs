using System;
using System.Linq;
using System.Text;

namespace UAlbion.Scripting.Ast;

public sealed record Statement(ICfgNode Head, params ICfgNode[] Parameters) : ICfgNode
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("S(");
        sb.Append(Head);
        if (Parameters != null)
        {
            foreach (var p in Parameters)
            {
                sb.Append(' ');
                sb.Append(p);
            }
        }
        sb.Append(')');

        return sb.ToString();
    }

    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;

    public bool Equals(Statement other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (!Head.Equals(other.Head)) return false;
        if (ReferenceEquals(Parameters, other.Parameters)) return true;
        return Parameters != null && other.Parameters != null && Parameters.SequenceEqual(other.Parameters);
    }

    public override int GetHashCode() => HashCode.Combine(Head, Parameters);
}