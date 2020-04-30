using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class PromptDialog : Dialog
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<PromptDialog, CloseWindowEvent>((x, e) => x.Respond(false))
        );

        public PromptDialog(IText text, int depth = 0) : base(Handlers, DialogPositioning.Center, depth)
        {
            var textSection = new TextElement(text).Center();

            UiElement buttons = new HorizontalStack(
                new Button(SystemTextId.MsgBox_Yes.ToId(), () => Respond(true)),
                new Spacing(8, 0),
                new Button(SystemTextId.MsgBox_No.ToId(), () => Respond(false)));

            var stack = new VerticalStack(
                new ButtonFrame(textSection) { State = ButtonState.Pressed },
                new Spacing(0, 5),
                buttons,
                new Spacing(180, 0));
            AttachChild(new DialogFrame(stack) { Background = DialogFrameBackgroundStyle.MainMenuPattern });
        }

        void Respond(bool response)
        {
            Response = response;
            Close();
        }

        void Close()
        {
            Detach();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
        public bool Response { get; private set; }
    }
}