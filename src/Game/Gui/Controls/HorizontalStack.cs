﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls
{
    /// <summary>
    /// Arranges a list of elements horizontally. The child elements share
    /// the available width evenly, excepting elements that implement
    /// IFixedSizeUiElement.
    /// </summary>
    public class HorizontalStack : UiElement
    {
        public HorizontalStack(params IUiElement[] args) : this((IList<IUiElement>)args) { }
        public HorizontalStack(IList<IUiElement> children)
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            foreach(var child in children)
                AttachChild(child);
        }

        public override Vector2 GetSize()
        {
            Vector2 size = Vector2.Zero;
            foreach (var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
            {
                var childSize = child.GetSize();
                size.X += childSize.X;

                if (childSize.Y > size.Y)
                    size.Y = childSize.Y;
            }

            return size;
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int maxOrder = order;
            int minWidth = 0;
            int nonFixedCount = 0;

            foreach(var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
            {
                int width = (int)child.GetSize().X;
                if (!(child is IFixedSizeUiElement))
                    nonFixedCount++;
                minWidth += width;
            }

            int spareWidth = extents.Width - minWidth;
            int offset = extents.X;
            foreach (var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
            {
                int width = (int)child.GetSize().X;
                var rect = new Rectangle(offset, extents.Y, width, extents.Height);
                if (!(child is IFixedSizeUiElement))
                    rect = new Rectangle(rect.X, rect.Y, rect.Width + spareWidth / nonFixedCount, rect.Height);

                maxOrder = Math.Max(maxOrder, func(child, rect, order));
                offset += rect.Width;
            }

            return maxOrder;
        }
    }
}
