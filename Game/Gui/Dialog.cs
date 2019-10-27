using System;
using System.Collections.Generic;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    public interface IDialog : IUiElement
    {
        int Depth { get; }
        DialogPositioning Positioning { get; }
    }

    public class Dialog : UiElement, IDialog
    {
        static IDictionary<Type, Handler> InjectDialogHandler(IDictionary<Type, Handler> handlers)
        {
            if (handlers.ContainsKey(typeof(CollectDialogsEvent)))
                return handlers;

            return new Dictionary<Type, Handler>(handlers)
            {
                { typeof(CollectDialogsEvent), new Handler<Dialog, CollectDialogsEvent>((x, e) => e.AddDialog(x)) }
            };
        }

        protected Dialog(IDictionary<Type, Handler> handlers, DialogPositioning position, int depth = 0)
            : base(InjectDialogHandler(handlers))
        {
            Positioning = position;
            Depth = depth;
        }

        public int Depth { get; }
        public DialogPositioning Positioning { get; }
    }
}