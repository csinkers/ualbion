using System;
using UAlbion.Core.Events;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    public class TextWindow : Dialog
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<TextWindow, UiLeftClickEvent>((x, e) => x.Close()),
            H<TextWindow, RightClickEvent>((x, e) => x.Close()),
            H<TextWindow, CloseDialogEvent>((x, e) => x.Close())
        );

        public TextWindow(ITextSource text, int depth = 0) : base(Handlers, DialogPositioning.Top, depth)
        {
            var textSection = new TextSection(text).Center();
            var padding = new Padding(textSection, 3, 7);
            var stack = new VerticalStack(padding, new Spacing(280, 0));
            AttachChild(new DialogFrame(stack));
        }

        void Close()
        {
            Detach();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
    }
}