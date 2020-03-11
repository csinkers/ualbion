using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    public interface IUiElement : IComponent
    {
        /// <summary>
        /// Reports the minimum required size for the element
        /// </summary>
        /// <returns></returns>
        Vector2 GetSize();

        /// <summary>
        /// Collects all renderables from the element and its children.
        /// </summary>
        /// <param name="extents">The rectangle to draw into, in UI coordinates</param>
        /// <param name="order">The render order to use</param>
        /// <returns>The maximum order rendered by any child</returns>
        int Render(Rectangle extents, int order);

        /// <summary>
        ///
        /// </summary>
        /// <param name="uiPosition"></param>
        /// <param name="extents"></param>
        /// <param name="order"></param>
        /// <param name="registerHitFunc"></param>
        int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc);
    }

    public interface IFixedSizeUiElement { } // Any elements with this interface won't get stretched to fill available space
}
