﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls
{
    public class ContextMenu : Dialog
    {
        ContextMenuEvent _event;
        Vector2 _uiPosition;

        public ContextMenu() : base(DialogPositioning.TopLeft, int.MaxValue)
        {
            On<ContextMenuEvent>(Display);
            On<CloseWindowEvent>(e => Display(null));
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            // Just the default condition without the extents check, as the use of a fixed position stack means the extents passed in are ignored.
            var maxOrder = DoLayout(extents, order, (x,y,z) => x.Select(uiPosition, y, z, registerHitFunc));
            registerHitFunc(order, this);
            return maxOrder;
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int maxOrder = order;
            foreach (var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
            {
                var size = child.GetSize();
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

                maxOrder = Math.Max(maxOrder, func(child, childExtents, order + 1));
            }
            return maxOrder;
        }

        void OnButton(ContextMenuOption option, bool keepOpen)
        {
            if (!keepOpen)
                Close();

            if (option.Event is IAsyncEvent asyncEvent)
                RaiseAsync(asyncEvent, null);
            else if (option.Event != null)
                Raise(option.Event);
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
                new HorizontalStack(new Spacing(5, 0), new BoldHeader(_event.Heading), new Spacing(5, 0)),
                new Divider(CommonColor.Yellow3),
                new Padding(new VerticalStack(optionElements), 0, 2)
            };

            var frame = new DialogFrame(new VerticalStack(elements));
            AttachChild(frame);
            Raise(new PushInputModeEvent(InputMode.ContextMenu));
        }
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
