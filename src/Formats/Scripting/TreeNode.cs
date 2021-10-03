using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class TreeNode<T>
    {
        public TreeNode(T value) => Value = value ?? throw new ArgumentNullException(nameof(value));
        TreeNode(T value, ImmutableList<TreeNode<T>> children)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Children = children;
        }

        public T Value { get; }
        public ImmutableList<TreeNode<T>> Children { get; } = ImmutableList<TreeNode<T>>.Empty;
        public TreeNode<T> AddChild(TreeNode<T> child) => new(Value, Children.Add(child));
        public TreeNode<T> RemoveChild(TreeNode<T> child)
        {
            var newChildren = Children.Remove(child);
            return ReferenceEquals(newChildren, Children) ? this : new(Value, newChildren);
        }

        TreeNode<T> ReplaceChild(TreeNode<T> oldChild, TreeNode<T> newChild) =>
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

        public TreeNode<T> FindChild(T value, Func<T, T, bool> equalityFunc, bool recursive = false)
        {
            foreach (TreeNode<T> child in Children)
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

        static TreeNode<T> AddPathInner(TreeNode<T> node, IList<T> path, int pathOffset, Func<T, T, bool> equalityFunc)
        {
            if (path.Count <= pathOffset)
                return node;

            TreeNode<T> child = node.FindChild(path[pathOffset], equalityFunc);

            return child == null 
                ? node.AddChild(AddPathInner(new TreeNode<T>(path[pathOffset]), path, pathOffset + 1, equalityFunc)) 
                : node.ReplaceChild(child, AddPathInner(child, path, pathOffset + 1, equalityFunc));
        }

#pragma warning disable CA1000 // Do not declare static members on generic types
        public static TreeNode<T> AddPath(TreeNode<T> tree, IList<T> path, Func<T, T, bool> equalityFunc)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (equalityFunc == null) throw new ArgumentNullException(nameof(equalityFunc));

            if (path.Count == 0)
                return tree;

            if (tree == null)
                tree = new TreeNode<T>(path[0]);
            else if (!equalityFunc(tree.Value, path[0]))
                throw new ArgumentException($"Path begins with \"{path[0]}\", but the tree is rooted at \"{tree.Value}\"");

            return AddPathInner(tree, path, 1, equalityFunc);
        }
#pragma warning restore CA1000 // Do not declare static members on generic types

        public bool Dominates(T a, T b, Func<T, T, bool> equalityFunc)
        {
            if (equalityFunc == null) throw new ArgumentNullException(nameof(equalityFunc));

            TreeNode<T> node = this;

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

        static void ToStringInner(StringBuilder sb, TreeNode<T> t, int level)
        {
            for (int i = 0; i < level - 1; i++)
                sb.Append("| ");
            if (level > 0)
                sb.Append("+-");

            sb.AppendLine(t.Value.ToString());

            foreach (TreeNode<T> child in t.Children)
                ToStringInner(sb, child, level + 1);
        }
    }
}