using System;
using System.Diagnostics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationTextWindow : ModalDialog
{
    readonly TextSourceWrapper _text = new();
    readonly UiText _uiText;
    AlbionTaskCore _source;

    public ConversationTextWindow(int depth) : base(DialogPositioning.Bottom, depth)
    {
        On<UiLeftClickEvent>(e => { Complete(); e.Propagating = false; });
        On<DismissMessageEvent>(_ => Complete());

        _uiText = new UiText(_text).Scrollable();

        // Transparent background, scrollable
        var content = new FixedSize(248, 159, new Padding(_uiText, 3));
        var frame = new DialogFrame(content) { Background = DialogFrameBackgroundStyle.DarkTint };
        AttachChild(frame);
    }

    public void Show(IText text, BlockId? blockFilter)
    {
        _uiText.BlockFilter = blockFilter;
        _text.Source = text;
    }

    public AlbionTask Closed()
    {
        Debug.Assert(_source == null, "Tried to start a new text window wait before the last one was completed");
        _source = new AlbionTaskCore("ConversationTextWindow");
        return _source.UntypedTask;
    }

    void Complete()
    {
        if (_uiText.ScrollOffset < _uiText.MaxScrollOffset)
        {
            ScrollDown();
            return;
        }

        var source = _source;
        _source = null;
        source?.Complete();
    }

    void ScrollDown()
    {
        var fromOffset = _uiText.ScrollOffset;
        var target = Math.Min(_uiText.MaxScrollOffset, _uiText.ScrollOffset + _uiText.PageSize);

        AttachChild(new AdHocComponent("ScrollTransition", x =>
        {
            var transitionTimeSeconds = 0.25f;
            var elapsedTime = 0.0f;

            x.On<EngineUpdateEvent>(e =>
            {
                elapsedTime += e.DeltaSeconds;
                float t = elapsedTime / transitionTimeSeconds;
                bool done = t > 1.0f;
                t = Math.Min(t, 1.0f);
                _uiText.ScrollOffset = (int)ApiUtil.Lerp(fromOffset, target, t);

                if (done)
                    x.Remove();
            });
        }));
    }
}