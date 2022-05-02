using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls;

public class UiBlocker : Component, IUiElement // Used to prevent the user from clicking any controls behind a modal dialog.
{
    public UiBlocker()
    {
        On<UiLeftClickEvent>(e => e.Propagating = false);
        On<UiRightClickEvent>(e => e.Propagating = false);
        On<UiScrollEvent>(e => e.Propagating = false);
    }

    public Vector2 GetSize() => UiConstants.UiExtents.Size;
    public int Render(Rectangle extents, int order) => order;
    public int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
    {
        if (registerHitFunc == null) throw new ArgumentNullException(nameof(registerHitFunc));
        registerHitFunc(order, this);
        return order;
    }

    public int Layout(Rectangle extents, int order, LayoutNode parent)
    {
        // Note: Construction is side-effectful: Adds the node to its parent's children.
        // ReSharper disable once AssignmentIsFullyDiscarded
        _ = new LayoutNode(parent, this, UiConstants.UiExtents, order);
        return order;
    }
}