using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class VerticalStack : Component, IUiElement
    {
        public VerticalStack(IList<IUiElement> children) : base(null)
        {
            foreach(var child in children)
                Children.Add(child);
        }
        public Vector2 Size
        {
            get
            {
                Vector2 size = Vector2.Zero;
                foreach (var child in Children.OfType<IUiElement>())
                {
                    var childSize = child.Size;
                    if (childSize.X > size.X)
                        size.X = childSize.X;

                    size.Y += childSize.Y;
                }
                return size;
            }
        }

        public void Render(Rectangle extents, Action<IRenderable> addFunc)
        {
            int offset = extents.Y;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int height = (int)child.Size.Y;
                child.Render(new Rectangle(extents.X,  offset, extents.Width, height), addFunc);
                offset += height;
            }
        }
    }
}