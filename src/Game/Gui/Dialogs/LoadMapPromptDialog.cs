using System;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class LoadMapPromptDialog : ModalDialog // TODO: Use textbox
{
    public LoadMapPromptDialog(IText text, int min, int max, int depth = 0) : base(DialogPositioning.Center, depth)
    {
        On<CloseWindowEvent>(_ => Close());

        var textSection = new UiText(text);
        var slider = new Slider(() => Value, x => Value = x, min, max);
        var button = new Button(Base.SystemText.MsgBox_OK).OnClick(Close);
        Value = min;

        // 30
        var stack = new VerticalStacker(
            new ButtonFrame(
                new HorizontalStacker(
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

    /*
    void Respond(int response)
    {
        Value = response;
        Close();
    }
    */

    void Close()
    {
        Remove();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs> Closed;
    public int Value { get; private set; }
}