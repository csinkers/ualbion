using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class NumericPromptDialog : ModalDialog
    {
        public NumericPromptDialog(IText text, int min, int max, int depth = 0) : base(DialogPositioning.Center, depth)
        {
            On<CloseWindowEvent>(e => Close());

            var textSection = new TextElement(text).Center();
            var slider = new Slider(() => Value, x => Value = x, min, max);
            var button = new Button(SystemTextId.MsgBox_OK.ToId(), Close);

            // 30
            var stack = new VerticalStack(
                new ButtonFrame(
                    new HorizontalStack(
                        textSection,
                        new Spacing(0, 31)))
                {
                    State = ButtonState.Pressed
                },
                new Spacing(0, 5),
                slider,
                new Spacing(0, 5),
                button,
                new Spacing(186, 0));
            AttachChild(new DialogFrame(stack) { Background = DialogFrameBackgroundStyle.MainMenuPattern });
        }

        void Respond(int response)
        {
            Value = response;
            Close();
        }

        void Close()
        {
            Detach();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
        public int Value { get; private set; }
    }
}