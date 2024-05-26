using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls;

public class UiBlocker : Component, IUiElement, ISelectionBlocker // Used to prevent the user from clicking any controls behind a modal dialog.
{
    public UiBlocker()
    {
        On<UiLeftClickEvent>(e => e.Propagating = false);
        On<UiRightClickEvent>(e => e.Propagating = false);
        On<UiScrollEvent>(e => e.Propagating = false);
    }

    public Vector2 GetSize() => UiConstants.UiExtents.Size;
    public int Render(Rectangle extents, int order, LayoutNode parent)
    {
        // Note: Construction is side-effectful: Adds the node to its parent's children.
        // ReSharper disable once AssignmentIsFullyDiscarded
        _ = parent == null ? null : new LayoutNode(parent, this, UiConstants.UiExtents, order);
        return order;
    }

    public int Selection(Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.AddHit(order, this);
        return order;
    }
}