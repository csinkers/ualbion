using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class DominatorTreeNode<T>
    {
        public DominatorTreeNode(T value) => Value = value ?? throw new ArgumentNullException(nameof(value));
        DominatorTreeNode(T value, ImmutableList<DominatorTreeNode<T>> children)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Children = children;
        }

        public T Value { get; }
        public ImmutableList<DominatorTreeNode<T>> Children { get; } = ImmutableList<DominatorTreeNode<T>>.Empty;
        public DominatorTreeNode<T> AddChild(DominatorTreeNode<T> child) => new(Value, Children.Add(child));
        public DominatorTreeNode<T> RemoveChild(DominatorTreeNode<T> child)
        {
            var newChildren = Children.Remove(child);
            return ReferenceEquals(newChildren, Children) ? this : new(Value, newChildren);
        }

        public DominatorTreeNode<T> ReplaceChild(DominatorTreeNode<T> oldChild, DominatorTreeNode<T> newChild) =>
            ReferenceEquals(oldChild, newChild) 
                ? this 
                : new(Value, Children.Replace(oldChild, newChild));

        public override string ToString() => $"{Value} ({Children.Count} children)";

        public IEnumerable<T> Values
        {
            get
            {
                yield return Value;
                foreach (var child in Children)
                    foreach (var value in child.Values)
                        yield return value;
            }
        }

        public DominatorTreeNode<T> FindChild(T value, Func<T, T, bool> equalityFunc, bool recursive = false)
        {
            foreach (DominatorTreeNode<T> child in Children)
            {
                if (equalityFunc(child.Value, value))
                    return child;

                if (!recursive) 
                    continue;

                var result = child.FindChild(value, equalityFunc, true);
                if (result != null!)
                    return result;
            }

            return null;
        }

        public bool Dominates(T a, T b, Func<T, T, bool> equalityFunc)
        {
            if (equalityFunc == null) throw new ArgumentNullException(nameof(equalityFunc));

            DominatorTreeNode<T> node = this;

            if (!equalityFunc(Value, a))
                node = FindChild(a, equalityFunc, true);

            return node.FindChild(b, equalityFunc, true) != null;
        }

        public string ToStringRecursive()
        {
            StringBuilder sb = new StringBuilder();
            ToStringInner(sb, this, 0);
            return sb.ToString();
        }

        static void ToStringInner(StringBuilder sb, DominatorTreeNode<T> t, int level)
        {
            for (int i = 0; i < level - 1; i++)
                sb.Append("| ");
            if (level > 0)
                sb.Append("+-");

            sb.AppendLine(t.Value.ToString());

            foreach (DominatorTreeNode<T> child in t.Children)
                ToStringInner(sb, child, level + 1);
        }
    }
}