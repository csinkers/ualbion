using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls;

public class ContextMenu : Dialog
{
    ContextMenuEvent _event;
    Vector2 _uiPosition;

    public ContextMenu() : base(DialogPositioning.TopLeft, int.MaxValue)
    {
        On<ContextMenuEvent>(Display);
        On<CloseWindowEvent>(_ => Display(null));
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        // Just the default condition without the extents check, as the use of a fixed position stack means the extents passed in are ignored.
        var maxOrder = DoLayout(extents, order, context, SelectChildDelegate);
        context.AddHit(order, this);
        return maxOrder;
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        int maxOrder = order;
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            var size = childElement.GetSize();
            int x = (int)_uiPosition.X;
            int y = (int)_uiPosition.Y;

            if (x + size.X > UiConstants.UiExtents.Right - 10)
                x -= (int)size.X;

            if (y + size.Y > UiConstants.UiExtents.Bottom - 10)
                y -= (int)size.Y;

            var childExtents = new Rectangle(
                x,
                y,
                (int)size.X,
                (int)size.Y);

            maxOrder = Math.Max(maxOrder, func(childElement, childExtents, order + 1, context));
        }
        return maxOrder;
    }

    void OnButton(ContextMenuOption option, bool keepOpen)
    {
        if (!keepOpen)
            Close();

        if (option.Event != null)
            _ = WithFrozenClock((this, option), static x => x.Item1.RaiseA(x.option.Event));
    }

    void Close()
    {
        RemoveAllChildren();
        _event = null;
        Raise(new PopInputModeEvent());
    }

    void Display(ContextMenuEvent contextMenuEvent)
    {
        if (_event != null)
            Close();

        if (contextMenuEvent == null)
            return;

        _event = contextMenuEvent;
        _uiPosition = contextMenuEvent.UiPosition;

        var optionElements = new List<IUiElement>();
        ContextMenuGroup? lastGroup = null;
        foreach (var option in _event.Options)
        {
            lastGroup ??= option.Group;
            if(lastGroup != option.Group)
                optionElements.Add(new Spacing(0, 2));
            lastGroup = option.Group;

            var option1 = option;
            optionElements.Add(new Button(option.Text).OnClick(() => OnButton(option1, option1.Disabled)));
        }

        var elements = new List<IUiElement>
        {
            new Spacing(0, 2),
            new HorizontalStacker(new Spacing(5, 0), new BoldHeader(_event.Heading), new Spacing(5, 0)),
            new Divider(CommonColor.Yellow3),
            new Padding(new VerticalStacker(optionElements), 0, 2)
        };

        var frame = new DialogFrame(new VerticalStacker(elements));
        AttachChild(frame);
        Raise(new PushInputModeEvent(InputMode.ContextMenu));
    }
}

/*
Map objects:
    Environment (header)
    divider
    Examine (white)
    Take (white)
    Manipulate (white)
    gap
    Rest (if restable map)
    Main menu (yellow)

NPC
    Person (header)
    divider
    Talk to (white)
    Main menu (yellow)

Dungeon objects:
    Environment (header)
    divider
    Examine (if examinable object)
    gap
    Map (yellow)
    Rest (yellow, if restable map)
    Main menu (yellow)

Status bar:
    Tom (header)
    divider
    Character screen (white)
    Use magic (white, if has magic)
    Make leader (white, if multiple players & not leader)
    Talk to (if not Tom)

Inventory:
    Gold / Rations (header)
    divider
    Throw away (white)

    item name (header)
    divider
    Drop (white)
    Examine (white)

Combat:
    Player
        Drirr (header)
        divider
        Do nothing (white)
        attack (white)
        Move (white)
        Use magic (white, if capable)
        Use magic item (white)
        Flee (if on bottom row)
        gap
        Advance party (white)
        gap
        Observe (yellow)
        Main menu (yellow)
    Enemy or empty
        Combat (header)
        divider
        Observe (yellow)
        Main menu (yellow)
*/
