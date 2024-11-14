using System;
using System.Numerics;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class TextPromptDialog : ModalDialog
{
    readonly DynamicText _textSource;
    readonly TextBlock _block;
    readonly UiRectangle _cursor;

    public TextPromptDialog(int depth) : base(DialogPositioning.Center, depth)
    {
        On<TextEntryCompleteEvent>(_ => Close());
        On<CloseWindowEvent>(_ => { Value = null; Close(); });
        On<TextEntryAbortEvent>(_ => { Value = null; Close(); });
        On<IdleClockEvent>(IdleTick);
        On<TextEntryCharEvent>(e => Value += e.Character);
        On<TextEntryBackspaceEvent>(_ =>
        {
            if (Value.Length > 0)
                Value = Value[..^1];
        });

        // outer frame dims = 183x34
        // TextBox: 162x13 (incl. border)
        // Cursor: 5x8, white, period ~280ms

        _block = new TextBlock("");
        _textSource = new DynamicText( () => [new TextBlock(Value)]);
        _cursor = new UiRectangle(CommonColor.Transparent)
        {
            DrawSize = new Vector2(5, 8),
            MeasureSize = new Vector2(5, 8),
        };

        var stack = new FixedSize(162, 13,
            new ButtonFrame(
                new HorizontalStacker(
                    new NonGreedy(new UiText(_textSource)),
                    _cursor,
                    new Spacing(0,0)))
            {
                State = ButtonState.Pressed
            });

        AttachChild(new DialogFrame(stack) { Background = DialogFrameBackgroundStyle.MainMenuPattern });
    }

    void IdleTick(IdleClockEvent obj)
    {
        bool set = _cursor.Color == CommonColor.White;
        _cursor.Color = set ? CommonColor.Transparent : CommonColor.White;
    }

    protected override void Subscribed()
    {
        Raise(new PushInputModeEvent(InputMode.TextEntry));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopInputModeEvent());
    }

    void Close()
    {
        Remove();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs> Closed;
    public string Value
    {
        get => _block.Text;
        private set
        {
            _block.Text = value;
            _textSource.Invalidate();
        }
    }
}