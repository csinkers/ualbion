using UAlbion.Api.Eventing;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationTextWindow : ModalDialog
{
    readonly TextSourceWrapper _text = new();
    readonly UiText _uiText;
    AlbionTaskSource _source;

    public ConversationTextWindow(int depth) : base(DialogPositioning.Bottom, depth)
    {
        On<UiLeftClickEvent>(_ => Complete());
        On<DismissMessageEvent>(_ => Complete());

        _uiText = new UiText(_text).Scrollable();

        // Transparent background, scrollable
        var content = new FixedSize(248, 159, new Padding(_uiText, 3));
        var frame = new DialogFrame(content) { Background = DialogFrameBackgroundStyle.DarkTint };
        AttachChild(frame);
    }

    public AlbionTask Show(IText text, BlockId? blockFilter)
    {
        // default filter = BlockId.MainText;

        _source = new AlbionTaskSource();
        _uiText.BlockFilter = blockFilter;
        _text.Source = text;
        return _source.Task;
    }

    void Complete()
    {
        _source?.Complete();
        _source = null;
    }
}