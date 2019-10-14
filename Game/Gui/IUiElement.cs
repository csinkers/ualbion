using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public interface IUiElement : IComponent
    {
        Vector2 GetSize();

        /// <summary>
        /// Collects all renderables from the element and its children.
        /// </summary>
        /// <param name="extents">The rectangle to draw into, in UI coordinates</param>
        /// <param name="order">The render order to use</param>
        /// <param name="addFunc">A function to report any renderables that need rendering</param>
        /// <returns>The maximum order rendered by any child</returns>
        int Render(Rectangle extents, int order, Action<IRenderable> addFunc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiPosition"></param>
        /// <param name="extents"></param>
        /// <param name="order"></param>
        /// <param name="registerHitFunc"></param>
        void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc);
    }
}
