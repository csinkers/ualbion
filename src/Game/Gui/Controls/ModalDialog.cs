using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

public class ModalDialog : Dialog
{
    readonly UiBlocker _blocker;
        
    protected ModalDialog(DialogPositioning position, int depth = 0) : base(position, depth)
        => _blocker = AttachChild(new UiBlocker());

    public override Vector2 GetSize()
    {
        Vector2 size = Vector2.Zero;
        if (Children == null) 
            return size;

        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            if (childElement == _blocker) // Don't include the blocker in the size calculation
                continue;
            var childSize = childElement.GetSize();
            if (childSize.X > size.X)
                size.X = childSize.X;
            if (childSize.Y > size.Y)
                size.Y = childSize.Y;
        }
        return size;
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        int maxOrder = order;
        if (extents.Contains((int)context.UiPosition.X, (int)context.UiPosition.Y))
        {
            foreach (var child in Children)
            {
                if (child is not IUiElement { IsActive: true } childElement)
                    continue;

                // Leave the blocker til last - we only want it to block windows beneath
                // this one, not children of this window.
                if (childElement == _blocker) 
                    continue;

                maxOrder = Math.Max(maxOrder, childElement.Selection(extents, order + 2, context));
            }

            // Add one to prevent a tie between the dialog and its UiBlocker
            context.HitFunc(order + 1, this);
        }

        _blocker.Selection(extents, order, context);
        return maxOrder;
    }
}