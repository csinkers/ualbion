using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace UAlbion.Scripting;

public class GenericTreeNode<T>
{
    public GenericTreeNode(T value) => Value = value ?? throw new ArgumentNullException(nameof(value));
    GenericTreeNode(T value, ImmutableList<GenericTreeNode<T>> children)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Children = children;
    }

    public T Value { get; }
    public ImmutableList<GenericTreeNode<T>> Children { get; } = ImmutableList<GenericTreeNode<T>>.Empty;
    public GenericTreeNode<T> AddChild(GenericTreeNode<T> child) => new(Value, Children.Add(child));
    public GenericTreeNode<T> RemoveChild(GenericTreeNode<T> child)
    {
        var newChildren = Children.Remove(child);
        return ReferenceEquals(newChildren, Children) ? this : new(Value, newChildren);
    }

    public GenericTreeNode<T> ReplaceChild(GenericTreeNode<T> oldChild, GenericTreeNode<T> newChild) =>
        ReferenceEquals(oldChild, newChild) 
            ? this 
            : new(Value, Children.Replace(oldChild, newChild));

    public GenericTreeNode<T> AddPath(IList<T> path, int pathOffset, Func<T, T, bool> equalityFunc)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (path.Count <= pathOffset)
            return this;

        GenericTreeNode<T> child = FindChild(path[pathOffset], equalityFunc);

        if (child == null)
        {
            child = new GenericTreeNode<T>(path[pathOffset]);
            return AddChild(child.AddPath(path, pathOffset + 1, equalityFunc));
        }
        else
            return ReplaceChild(child, child.AddPath(path, pathOffset + 1, equalityFunc));
    }

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

    public GenericTreeNode<T> FindChild(T value, Func<T, T, bool> equalityFunc, bool recursive = false)
    {
        if (equalityFunc == null) throw new ArgumentNullException(nameof(equalityFunc));

        foreach (GenericTreeNode<T> child in Children)
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

    public List<T> FindPath(T value, Func<T, T, bool> equalityFunc)
    {
        if (equalityFunc == null) throw new ArgumentNullException(nameof(equalityFunc));

        foreach (GenericTreeNode<T> child in Children)
        {
            if (equalityFunc(child.Value, value))
                return new List<T> { child.Value };

            var result = child.FindPath(value, equalityFunc);
            if (result != null)
            {
                result.Add(child.Value);
                return result;
            }
        }

        return null;
    }

    public string ToStringRecursive()
    {
        StringBuilder sb = new StringBuilder();
        ToStringInner(sb, this, 0);
        return sb.ToString();
    }

    static void ToStringInner(StringBuilder sb, GenericTreeNode<T> t, int level)
    {
        for (int i = 0; i < level - 1; i++)
            sb.Append("| ");
        if (level > 0)
            sb.Append("+-");

        sb.AppendLine(t.Value.ToString());

        foreach (GenericTreeNode<T> child in t.Children)
            ToStringInner(sb, child, level + 1);
    }
}