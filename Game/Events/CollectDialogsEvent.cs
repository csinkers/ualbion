using System;
using UAlbion.Api;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Events
{
    public class CollectDialogsEvent : GameEvent, IVerboseEvent
    {
        public Action<IDialog> AddDialog { get; }

        public CollectDialogsEvent(Action<IDialog> addDialog) { AddDialog = addDialog; }
    }
}