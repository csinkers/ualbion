using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls;

/// <summary>
/// Renders only the 1-pixel border of a rectangle (no fill), using four thin UiRectangle strips.
/// Returns the actual (w,h) size so the layout allocates proper space and strips are not clipped.
/// </summary>
public class UiBorderRect : UiElement
{
    readonly UiRectangle _top;
    readonly UiRectangle _bottom;
    readonly UiRectangle _left;
    readonly UiRectangle _right;
    readonly int _w, _h;

    public UiBorderRect(CommonColor color, int w, int h)
    {
        _w = w; _h = h;
        _top    = new UiRectangle(color) { DrawSize = new Vector2(w, 1),     MeasureSize = Vector2.Zero };
        _bottom = new UiRectangle(color) { DrawSize = new Vector2(w, 1),     MeasureSize = Vector2.Zero };
        _left   = new UiRectangle(color) { DrawSize = new Vector2(1, h - 2), MeasureSize = Vector2.Zero };
        _right  = new UiRectangle(color) { DrawSize = new Vector2(1, h - 2), MeasureSize = Vector2.Zero };
        AttachChild(_top);
        AttachChild(_bottom);
        AttachChild(_left);
        AttachChild(_right);
    }

    // Return actual size so parent layout allocates space and strips are not clipped.
    public override Vector2 GetSize() => new(_w, _h);

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        _top.Render(   new Rectangle(extents.X,                      extents.Y,                       extents.Width,  1), order, parent);
        _bottom.Render(new Rectangle(extents.X,                      extents.Y + extents.Height - 1,  extents.Width,  1), order, parent);
        _left.Render(  new Rectangle(extents.X,                      extents.Y + 1,                   1,              extents.Height - 2), order, parent);
        _right.Render( new Rectangle(extents.X + extents.Width - 1,  extents.Y + 1,                   1,              extents.Height - 2), order, parent);
        return order;
    }
}
