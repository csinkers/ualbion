using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
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

        public YesNoMessageBox(StringId stringId) : base(Handlers)
        {
            var elements = new VerticalStack(
                new Padding(0, 5),
                new FixedSizePanel(231, 30, new Text(stringId)),
                new Padding(0, 5),
                new HorizontalStack(
                    new Padding(11, 0),
                    new Button(YesButtonKey, S(SystemTextId.MsgBox_Yes)) { DoubleFrame = true },
                    new Padding(8, 0),
                    new Button(NoButtonKey, S(SystemTextId.MsgBox_No)) { DoubleFrame = true },
                    new Padding(10, 0)
                ),
                new Padding(0, 5)
            );

            var horizontalPad = new HorizontalStack(
                new Padding(6, 0),
                elements,
                new Padding(6, 0)
            );

            Children.Add(new DialogFrame(horizontalPad));
        }

        public event EventHandler Closed;
        public bool Result { get; private set; }
    }
}