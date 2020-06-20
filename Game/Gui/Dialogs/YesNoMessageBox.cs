using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    /// <summary>
    /// Show a yes/no dialog box. These should only be created by DialogManager, if any code
    /// needs to prompt the player for a yes/no answer it should raise a YesNoPromptEvent.
    /// </summary>
    class YesNoMessageBox : ModalDialog
    {
        void OnButton(bool isYes)
        {
            Result = isYes;
            Remove();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public YesNoMessageBox(StringId stringId, int depth) : base(DialogPositioning.Center, depth)
        {
            var elements = new VerticalStack(
                new Spacing(0, 5),
                new FixedSizePanel(231, 30, new UiTextBuilder(stringId)),
                new Spacing(0, 5),
                new HorizontalStack(
                    new Spacing(11, 0),
                    new Button(SystemTextId.MsgBox_Yes) { DoubleFrame = true }
                        .OnClick(() => OnButton(true)),
                    new Spacing(8, 0),
                    new Button(SystemTextId.MsgBox_No) { DoubleFrame = true }
                        .OnClick(() => OnButton(false)),
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
