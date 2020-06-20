using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class PromptDialog : ModalDialog
    {
        public PromptDialog(IText text, int depth = 0) : base(DialogPositioning.Center, depth)
        {
            On<CloseWindowEvent>(e => Respond(false));

            var textSection = new UiText(text);

            UiElement buttons = new HorizontalStack(
                new Button(SystemTextId.MsgBox_Yes).OnClick(() => Respond(true)),
                new Spacing(8, 0),
                new Button(SystemTextId.MsgBox_No).OnClick(() => Respond(false)));

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
            Remove();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
        public bool Response { get; private set; }
    }
}