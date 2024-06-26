﻿using System;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

public class CentreContent : UiElement, IFixedSizeUiElement
{
    readonly UiElement _content;

    public CentreContent(UiElement content)
    {
        AttachChild(content);
        _content = content;
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        var childSize = _content.GetSize();
        var childExtents = new Rectangle(
            extents.X + (int)(extents.Width - childSize.X) / 2,
            extents.Y + (int)(extents.Height - childSize.Y) / 2,
            (int)childSize.X,
            (int)childSize.Y);

        return Math.Max(order, func(_content, childExtents, order + 1, context));
    }
}