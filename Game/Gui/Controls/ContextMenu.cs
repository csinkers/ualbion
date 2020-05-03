using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls
{
    public class ContextMenu : Dialog
    {
        ContextMenuEvent _event;

        public ContextMenu() : base(DialogPositioning.TopLeft)
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

        void OnButton(ContextMenuOption option)
        {
            Close();
            Raise(option.Event);
        }

        void Close()
        {
            foreach (var child in Children)
                child.Detach();
            Children.Clear();
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
            var elements = new List<IUiElement>
            {
                new Spacing(0, 2),
                new HorizontalStack(new Spacing(5, 0), new Header(_event.Heading), new Spacing(5, 0)),
                new Divider(CommonColor.Yellow3),
                new Spacing(0, 2),
            };

            ContextMenuGroup? lastGroup = null;
            foreach (var option in _event.Options)
            {
                lastGroup ??= option.Group;
                if(lastGroup != option.Group)
                    elements.Add(new Spacing(0, 2));
                lastGroup = option.Group;

                var option1 = option;
                elements.Add(new Button(option.Text, () => OnButton(option1)));
            }

            var frame = new DialogFrame(new VerticalStack(elements));
            var fixedStack = new FixedPositionStack();
            fixedStack.Add(frame, (int)contextMenuEvent.UiPosition.X, (int)contextMenuEvent.UiPosition.Y);
            AttachChild(fixedStack);
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
