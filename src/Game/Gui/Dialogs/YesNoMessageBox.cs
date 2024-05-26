using System;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

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

    static TextFilter BuildButtonText(IText text, BlockId blockNumber, TextId fallback, ITextFormatter textFormatter) =>
        new(x => { x.Alignment = TextAlignment.Center; return true; })
            {
                Source = 
                    new TextFallback(
                        new TextFilter(x => x.BlockId == blockNumber) { Source = text },
                        textFormatter.Format(fallback))
            };

    public YesNoMessageBox(StringId stringId, int depth, ITextFormatter textFormatter) : base(DialogPositioning.Center, depth)
    {
        On<RespondEvent>(e =>
        {
            if (e.Option != 1 && e.Option != 2) return;
            OnButton(e.Option == 1);
        });

        var text = textFormatter.Format(stringId);
        var body = new TextFilter(x => x.BlockId == BlockId.MainText) { Source = text };
        var yesText = BuildButtonText(text, (BlockId)10, Base.SystemText.MsgBox_Yes, textFormatter);
        var noText = BuildButtonText(text, (BlockId)20, Base.SystemText.MsgBox_No, textFormatter);

        // TODO: Block0 = yes text, Block1 = no text.
        var elements = new VerticalStacker(
            new Spacing(0, 5),
            new FixedSizePanel(231, 30, new UiText(body)),
            new Spacing(0, 5),
            new HorizontalStacker(
                new Spacing(11, 0),
                new Button(yesText) { DoubleFrame = true }
                    .OnClick(() => OnButton(true)),
                new Spacing(8, 0),
                new Button(noText) { DoubleFrame = true }
                    .OnClick(() => OnButton(false)),
                new Spacing(10, 0)
            ),
            new Spacing(0, 5)
        );

        var horizontalPad = new HorizontalStacker(
            new Spacing(6, 0),
            elements,
            new Spacing(6, 0)
        );

        AttachChild(new DialogFrame(horizontalPad));
    }

    public event EventHandler Closed;
    public bool Result { get; private set; }
}