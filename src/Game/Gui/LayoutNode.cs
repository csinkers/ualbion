using System;
using System.Collections.Generic;
using UAlbion.Core;

namespace UAlbion.Game.Gui;

public class LayoutNode
{
    readonly List<LayoutNode> _children = new();
    public LayoutNode(LayoutNode parent, IUiElement element, Rectangle extents, int order)
    {
        Children = _children.AsReadOnly();
        Parent = parent;
        Element = element;
        Extents = extents;
        Order = order;
        parent?._children.Add(this);
    }

    public IUiElement Element { get; }
    public Rectangle Extents { get; }
    public int Order { get; }
    public LayoutNode Parent { get; }
    public IReadOnlyList<LayoutNode> Children { get; }

    public IEnumerable<LayoutNode> DepthFirstSearch(Func<LayoutNode, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        foreach(var child in Children)
        {
            if (predicate(child))
                yield return child;

            foreach (var childResult in child.DepthFirstSearch(predicate))
                yield return childResult;
        }
    }

    public IEnumerable<LayoutNode> Ancestors
    {
        get
        {
            var node = Parent;
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }
    }

    public override string ToString() => $"N:{Element}";
}