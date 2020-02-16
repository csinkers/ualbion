using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    public class ContextMenuEvent : GameEvent
    {
        public ContextMenuEvent(Vector2 position, ITextSource heading, IEnumerable<ContextMenuOption> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            Heading = heading ?? throw new ArgumentNullException(nameof(heading));
            Position = position;
            Options = new ReadOnlyCollection<ContextMenuOption>(
                options
                    .OrderBy(x => x.Group)
                    .ToList());
        }

        public Vector2 Position { get; }
        public ITextSource Heading { get; }
        public IReadOnlyList<ContextMenuOption> Options { get; }
    }

    public class ContextMenu : Dialog
    {
        const string ButtonKeyPattern = "Context.Option";
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ContextMenu, ContextMenuEvent>((x, e) => { x.Display(e); }),
            H<ContextMenu, ButtonPressEvent>((x, e) => x.OnButton(e.ButtonId)),
            H<ContextMenu, CloseDialogEvent>((x, e) => x.Display(null))
        );

        ContextMenuEvent _event;

        public ContextMenu() : base(Handlers, DialogPositioning.TopLeft) { }

        void OnButton(string buttonId)
        {
            if (_event == null || !buttonId.StartsWith(ButtonKeyPattern))
                return;

            if (!int.TryParse(buttonId.Substring(ButtonKeyPattern.Length), out var id) ||
                id >= _event.Options.Count)
            {
                Raise(new LogEvent(LogEvent.Level.Warning, $"Out of range context menu button event received: {buttonId} ({_event.Options.Count} context elements)"));
                return;
            }

            var option = _event.Options[id];
            Raise(option.Event);
            Close();
        }

        void Close()
        {
            foreach (var child in Children)
                child.Detach();
            Children.Clear();
            _event = null;
        }

        void Display(ContextMenuEvent contextMenuEvent)
        {
            Close();
            if (contextMenuEvent == null)
                return;

            _event = contextMenuEvent;
            var elements = new List<IUiElement>
            {
                new Padding(0, 2),
                new HorizontalStack(new Padding(5, 0), new Header(_event.Heading), new Padding(5, 0)),
                new Divider(CommonColor.Yellow3),
                new Padding(0, 2),
            };

            ContextMenuGroup? lastGroup = null;
            for(int i = 0; i < _event.Options.Count; i++)
            {
                var option = _event.Options[i];
                lastGroup ??= option.Group;
                if(lastGroup != option.Group)
                    elements.Add(new Padding(0, 2));
                lastGroup = option.Group;

                elements.Add(new Button(ButtonKeyPattern + i, option.Text));
            }

            var stack = new VerticalStack(elements);
            Children.Add(new DialogFrame(stack));

            foreach (var child in Children)
                child.Attach(Exchange);
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
