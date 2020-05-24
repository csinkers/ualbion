using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    class YesNoMessageBox : ModalDialog
    {
        void OnButton(bool isYes)
        {
            Result = isYes;
            Detach();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public YesNoMessageBox(StringId stringId) : base(DialogPositioning.Center)
        {
            var elements = new VerticalStack(
                new Spacing(0, 5),
                new FixedSizePanel(231, 30, new UiTextBuilder(stringId)),
                new Spacing(0, 5),
                new HorizontalStack(
                    new Spacing(11, 0),
                    new Button(SystemTextId.MsgBox_Yes.ToId(), () => OnButton(true)) { DoubleFrame = true },
                    new Spacing(8, 0),
                    new Button(SystemTextId.MsgBox_No.ToId(), () => OnButton(false)) { DoubleFrame = true },
                    new Spacing(10, 0)
                ),
                new Spacing(0, 5)
            );

            var horizontalPad = new HorizontalStack(
                new Spacing(6, 0),
                elements,
                new Spacing(6, 0)
            );

            AttachChild(new DialogFrame(horizontalPad));
        }

        public event EventHandler Closed;
        public bool Result { get; private set; }
    }
}
