using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Events;

public class CollectDialogsEvent : GameEvent, IVerboseEvent
{
    public List<IDialog> Dialogs { get; } = new();
}