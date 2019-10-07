using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public interface IUiElement : IComponent
    {
        Vector2 Size { get; }
        /*
        {
            Vector2 size = Vector2.Zero;
            if (Children != null)
            {
                foreach (var child in Children.OfType<IUiElement>())
                {
                    var childSize = child.Size;
                    if (childSize.X > size.X)
                        size.X = childSize.X;
                    if (childSize.Y > size.Y)
                        size.Y = childSize.Y;
                }
            }
            return size;
        }*/

        void Render(Rectangle extents, Action<IRenderable> addFunc);
    }

    /*
    Arrangers:
        Vertical list (all widths set to max required width, all heights independently set based on required heights)
        Hardcoded
        Hardcoded width, height based on required height.
        Grid of fixed size elements
     */
}
