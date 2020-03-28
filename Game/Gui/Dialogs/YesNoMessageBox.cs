using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    class YesNoMessageBox : Dialog
    {
        const string YesButtonKey = "YesNoDialog.YesButton";
        const string NoButtonKey = "YesNoDialog.NoButton";
        static StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);

        static readonly HandlerSet Handlers = new HandlerSet(
            H<YesNoMessageBox, ButtonPressEvent>((x, e) =>
            {
                switch(e.ButtonId)
                {
                    case YesButtonKey:
                        x.Result = true;
                        x.Detach();
                        x.Closed?.Invoke(x, EventArgs.Empty);
                        break;

                    case NoButtonKey:
                        x.Result = false;
                        x.Detach();
                        x.Closed?.Invoke(x, EventArgs.Empty);
                        break;
                }
            })
        );

        public YesNoMessageBox(StringId stringId) : base(Handlers, DialogPositioning.Center)
        {
            var elements = new VerticalStack(
                new Spacing(0, 5),
                new FixedSizePanel(231, 30, new TextBlockElement(stringId)),
                new Spacing(0, 5),
                new HorizontalStack(
                    new Spacing(11, 0),
                    new Button(YesButtonKey, S(SystemTextId.MsgBox_Yes)) { DoubleFrame = true },
                    new Spacing(8, 0),
                    new Button(NoButtonKey, S(SystemTextId.MsgBox_No)) { DoubleFrame = true },
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
