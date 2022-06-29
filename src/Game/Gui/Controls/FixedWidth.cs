using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

public class FixedWidth : UiElement, IFixedSizeUiElement
{
    readonly int _width;

    public FixedWidth(int width, IUiElement child)
    {
        _width = width;
        AttachChild(child);
    }

    public DialogPositioning Position { get; set; } = DialogPositioning.Center;

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        int shiftX = Math.Max(0, extents.Width - _width);

        shiftX = Position switch
        {
            DialogPositioning.Center      => shiftX/2,
            DialogPositioning.Top         => shiftX/2,
            DialogPositioning.Left        => 0,
            DialogPositioning.Right       => shiftX,
            DialogPositioning.BottomLeft  => 0,
            DialogPositioning.TopLeft     => 0,
            DialogPositioning.TopRight    => shiftX,
            DialogPositioning.BottomRight => shiftX,
            DialogPositioning.Bottom      => shiftX/2,
            _ => shiftX / 2
        };

        var fixedExtents = new Rectangle(extents.X + shiftX, extents.Y, _width, extents.Height);
        return base.DoLayout(fixedExtents, order, context, func);
    }

    public override Vector2 GetSize() => new(_width, base.GetSize().Y);
    public override string ToString() => $"FixedWidth: <{_width}> {Position} {Children[0]}";
}