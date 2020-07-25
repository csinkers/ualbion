using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class TextDialog : ModalDialog
    {
        public TextDialog(IText text, SmallPortraitId? portraitId = null, int depth = 0) : base(DialogPositioning.Top, depth)
        {
            On<DismissMessageEvent>(_ => Close());
            On<UiLeftClickEvent>(e => { Close(); e.Propagating = false; });
            On<UiRightClickEvent>(e => { Close(); e.Propagating = false; });
            On<CloseWindowEvent>(e => Close());

            var textSection = new UiText(text);
            var padding = new Padding(textSection, 3, 7);

            UiElement content;
            if (portraitId.HasValue)
            {
                var portrait = new FixedSize(36, 38,
                    new ButtonFrame(new UiSpriteElement<SmallPortraitId>(portraitId.Value))
                    {
                        State = ButtonState.Pressed,
                        Padding = 0
                    });
                content = new HorizontalStack( new CentreContent(portrait), padding);
            }
            else
                content = padding;

            var stack = new FixedWidth(320, content);
            AttachChild(new DialogFrame(stack) { Background = DialogFrameBackgroundStyle.DarkTint });
        }

        void Close()
        {
            Remove();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
    }
}
