using System;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

public class NonGreedy : UiElement, IFixedSizeUiElement
{
    public NonGreedy(IUiElement child) => AttachChild(child);
    public DialogPositioning Position { get; set; } = DialogPositioning.Center;

    protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
    {
        var size = ((IUiElement)Children[0]).GetSize();
        int shiftX = Math.Max(0, extents.Width - (int)size.X);
        int shiftY = Math.Max(0, extents.Height - (int)size.Y);

        (shiftX, shiftY) = Position switch
        {
            DialogPositioning.Center      => (shiftX/2, shiftY/2),
            DialogPositioning.Top         => (shiftX/2, 0),
            DialogPositioning.Left        => (0, shiftY),
            DialogPositioning.Right       => (shiftX, shiftY),
            DialogPositioning.BottomLeft  => (0, shiftY),
            DialogPositioning.TopLeft     => (0, 0),
            DialogPositioning.TopRight    => (shiftX, 0),
            DialogPositioning.BottomRight => (shiftX, shiftY),
            DialogPositioning.Bottom      => (shiftX/2, shiftY),
            _ => (shiftX / 2, shiftY / 2),
        };

        var fixedExtents = new Rectangle(extents.X + shiftX, extents.Y + shiftY, (int)size.X, (int)size.Y);
        return base.DoLayout(fixedExtents, order, func);
    }

    public override string ToString() => $"NonGreedy: {Position} {Children[0]}";
}