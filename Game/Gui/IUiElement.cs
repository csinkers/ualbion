using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    /// <summary>
    /// The common interface shared by all user interface controls.
    /// </summary>
    public interface IUiElement : IComponent
    {
        /// <summary>
        /// Reports the minimum required size for the element
        /// </summary>
        /// <returns></returns>
        Vector2 GetSize();

        /// <summary>
        /// Collects all renderables from the element and its children and
        /// performs layout.
        /// </summary>
        /// <param name="extents">The rectangle to draw into, in UI coordinates</param>
        /// <param name="order">The render order to use</param>
        /// <returns>The maximum order rendered by any child</returns>
        int Render(Rectangle extents, int order);

        /// <summary>
        /// Used to discover all UI elements that occupy a given point on the
        /// screen in UI coordinates, typically to facilitate mouse events.
        /// </summary>
        /// <param name="uiPosition">The position being probed</param>
        /// <param name="extents">The rectangle the element would normally draw into</param>
        /// <param name="order">The render order that would be used</param>
        /// <param name="registerHitFunc">A callback for elements containing the probe
        /// point to call to alert the caller to their presence</param>
        int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc);

        /// <summary>
        /// Used to build a logical representation of the screen layout for debugging and testing purposes.
        /// </summary>
        /// <param name="extents">The rectangle the element would normally draw into</param>
        /// <param name="order">The render order that would be used</param>
        /// <param name="parent">The LayoutNode corresponding to this element's parent element.</param>
        int Layout(Rectangle extents, int order, LayoutNode parent);
    }

    public interface IFixedSizeUiElement { } // Any elements with this interface won't get stretched to fill available space
}
