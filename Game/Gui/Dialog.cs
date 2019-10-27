using System;
using System.Collections.Generic;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    public class Dialog : UiElement
    {
        static IDictionary<Type, Handler> InjectDialogHandler(IDictionary<Type, Handler> handlers)
        {
            if (handlers.ContainsKey(typeof(CollectDialogsEvent)))
                return handlers;

            var newHandlers = new Dictionary<Type, Handler>(handlers);
            newHandlers.Add(typeof(CollectDialogsEvent), 
                new Handler<Dialog, CollectDialogsEvent>((x,e) => e.AddDialog((x, x.Positioning))));
            return newHandlers;
        }

        protected Dialog(IDictionary<Type, Handler> handlers) : base(InjectDialogHandler(handlers)) { }
        protected DialogPositioning Positioning { get; set; } = DialogPositioning.Center;
    }
}