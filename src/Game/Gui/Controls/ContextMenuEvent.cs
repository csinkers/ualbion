using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls;

public class ContextMenuEvent : GameEvent
{
    public ContextMenuEvent(Vector2 uiPosition, IText heading, IEnumerable<ContextMenuOption> options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        Heading = heading ?? throw new ArgumentNullException(nameof(heading));
        UiPosition = uiPosition;
        Options = options.OrderBy(x => x.Group).ToList().AsReadOnly();
    }

    public Vector2 UiPosition { get; }
    public IText Heading { get; }
    public IReadOnlyList<ContextMenuOption> Options { get; }
}