using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

public class FixedPosition : UiElement, IFixedSizeUiElement
{
    readonly Rectangle _extents;

    public FixedPosition(Rectangle extents, IUiElement child)
    {
        _extents = extents;
        if (child != null)
            AttachChild(child);
    }

    public override Vector2 GetSize() => new(_extents.Width, _extents.Height);
    public override int Render(Rectangle extents, int order, LayoutNode parent) => base.Render(_extents, order, parent);
    public override int Selection(Rectangle extents, int order, SelectionContext context) => base.Selection(_extents, order, context);
    public override string ToString() => $"FixedPosition: {_extents} {Children[0]}";
}