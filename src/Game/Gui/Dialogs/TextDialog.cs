using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class TextDialog : ModalDialog
{
    readonly AlbionTaskCore _source = new("TextDialog");
    public AlbionTask Task => _source.UntypedTask;

    public TextDialog(IText text, SpriteId portraitId = default, int depth = 0) : base(DialogPositioning.Top, depth)
    {
        On<DismissMessageEvent>(_ => Close());
        On<UiLeftClickEvent>(e => { Close(); e.Propagating = false; });
        On<UiRightClickEvent>(e => { Close(); e.Propagating = false; });
        On<CloseWindowEvent>(_ => Close());

        var textSection = new UiText(text);
        var padding = new Padding(textSection, 3, 7);

        UiElement content;
        if (!portraitId.IsNone)
        {
            var portrait = new FixedSize(36, 38,
                new ButtonFrame(new UiSpriteElement(portraitId))
                {
                    State = ButtonState.Pressed,
                    Padding = 0
                });
            content = new HorizontalStacker( new CentreContent(portrait), padding);
        }
        else
            content = padding;

        var stack = new FixedWidth(320, content);
        AttachChild(new DialogFrame(stack) { Background = DialogFrameBackgroundStyle.DarkTint });
    }

    void Close()
    {
        Remove();
        _source.Complete();
    }
}