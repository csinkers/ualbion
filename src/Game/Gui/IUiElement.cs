using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game.Gui;

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
    /// <param name="parent">Debugging only: The LayoutNode corresponding to this element's parent element.</param>
    /// <returns>The maximum order rendered by any child</returns>
    int Render(Rectangle extents, int order, LayoutNode parent);

    /// <summary>
    /// Used to discover all UI elements that occupy a given point on the
    /// screen in UI coordinates, typically to facilitate mouse events.
    /// </summary>
    /// <param name="extents">The rectangle the element would normally draw into</param>
    /// <param name="order">The render order that would be used</param>
    /// <param name="c">The selection context, containing the cursor position and
    /// a callback for elements containing the cursor to call to alert the
    /// caller to their presence</param>
    int Select(Rectangle extents, int order, SelectionContext c);

    /// <summary>
    /// A delegate for hit-reporting callbacks
    /// </summary>
    /// <param name="order"></param>
    /// <param name="element"></param>
    public delegate void RegisterHitFunc(int order, object element);
}