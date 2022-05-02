using System;
using UAlbion.Api.Eventing;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Events;

public class CollectDialogsEvent : GameEvent, IVerboseEvent
{
    public Action<IDialog> AddDialog { get; }

    public CollectDialogsEvent(Action<IDialog> addDialog) { AddDialog = addDialog; }
}