using System;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationTextWindow : ModalDialog
{
    readonly TextSourceWrapper _text = new();
    readonly UiText _uiText;

    public event Action Clicked;

    public ConversationTextWindow() : base(DialogPositioning.Bottom)
    {
        On<UiLeftClickEvent>(_ => Clicked?.Invoke());
        On<DismissMessageEvent>(_ => Clicked?.Invoke());

        _uiText = new UiText(_text).Scrollable();

        // Transparent background, scrollable
        var content = new FixedSize(248, 159, new Padding(_uiText, 3));
        var frame = new DialogFrame(content) { Background = DialogFrameBackgroundStyle.DarkTint };
        BlockFilter = Conversation.SpecialBlockId.MainText;
        AttachChild(frame);
    }

    public IText Text
    {
        get => _text.Source;
        set => _text.Source = value;
    }

    public Conversation.SpecialBlockId BlockFilter
    {
        get => (Conversation.SpecialBlockId)_uiText.BlockFilter;
        set => _uiText.BlockFilter = (int)value;
    }
}