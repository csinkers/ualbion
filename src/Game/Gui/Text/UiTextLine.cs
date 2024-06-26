﻿using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class UiTextLine : UiElement // Multiple TextChunks arranged on a line
{
    public int Width { get; private set; }
    public int Height { get; private set; } = 8;

    readonly Rectangle? _scissorRegion;
    TextAlignment _alignment;

    public UiTextLine(Rectangle? scissorRegion) => _scissorRegion = scissorRegion;

    /// <summary>
    /// Add a new block to the line.
    /// </summary>
    /// <param name="block"></param>
    /// <param name="size"></param>
    public void Add(TextBlock block, Vector2 size)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (string.IsNullOrEmpty(block.Text))
            return;

        Width += (int)size.X;
        Height = Math.Max(Height, (int)size.Y);
        _alignment = block.Alignment;

        var lastChild = Children.OfType<UiTextBlock>().LastOrDefault(x => x.IsActive);
        if(lastChild != null && block.IsMergeableWith(lastChild.Block))
        {
            lastChild.Block.Merge(block);
            lastChild.IsDirty = true;
        }
        else
        {
            AttachChild(new UiTextBlock(block, _scissorRegion));
        }
    }

    public override Vector2 GetSize()
    {
        var size = base.GetSize();
        return new Vector2(Math.Max(size.X, Width), Math.Max(size.Y, Height));
    }

    public override string ToString() =>
        "UiTextLine:[ " +
        string.Join("; ", Children.OfType<UiTextBlock>().Where(x => x.IsActive).Select(x => x.ToString()))
        + " ]";

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var lineExtents = _alignment switch
        {
            TextAlignment.Center => new Rectangle(extents.X + (extents.Width - Width) / 2, extents.Y, Width, Height),
            TextAlignment.Right => new Rectangle(extents.X + (extents.Width - Width), extents.Y, Width, Height),
            _ => new Rectangle(extents.X, extents.Y, Width, Height)
        };

        int maxOrder = order;
        int offset = 0;
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } chunk)
                continue;

            var size = chunk.GetSize();
            maxOrder = Math.Max(maxOrder, func(chunk,
                new Rectangle(
                    lineExtents.X + offset,
                    (int)(lineExtents.Y + lineExtents.Height - size.Y),
                    (int)size.X,
                    (int)size.Y),
                order + 1, context));
            offset += (int)size.X;
        }

        return maxOrder;
    }
}