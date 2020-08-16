using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls
{
    /// <summary>
    /// Arranges a list of elements such that all elements share the
    /// same area, but elements that appear later in the child list
    /// are rendered above the elements that appear earlier.
    /// </summary>
    public class LayerStack : UiElement
    {
        public LayerStack(params IUiElement[] children) : this((IList<IUiElement>)children) { }
        public LayerStack(IList<IUiElement> children)
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            foreach(var child in children)
                Children.Add(child);
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            foreach(var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
            {
                var childSize = child.GetSize();
                var childExtents = new Rectangle(
                    extents.X,
                    extents.Y,
                    (int)childSize.X,
                    (int)childSize.Y);

                order = Math.Max(order, func(child, childExtents, order + 1));
            }
            return order;
        }
    }
}
