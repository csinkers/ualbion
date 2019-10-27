using System;
using UAlbion.Api;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Events
{
    public class CollectDialogsEvent : GameEvent, IVerboseEvent
    {
        public Action<(IUiElement, DialogPositioning)> AddDialog { get; }

        public CollectDialogsEvent(Action<(IUiElement, DialogPositioning)> addDialog) { AddDialog = addDialog; }
    }
}