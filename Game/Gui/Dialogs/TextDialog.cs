using System;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class TextDialog : Dialog
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<TextDialog, UiLeftClickEvent>((x, e) => x.Close()),
            H<TextDialog, RightClickEvent>((x, e) => x.Close()),
            H<TextDialog, CloseWindowEvent>((x, e) => x.Close())
        );

        public TextDialog(IText text, SmallPortraitId? portraitId = null, int depth = 0) : base(Handlers, DialogPositioning.Top, depth)
        {
            var textSection = new TextElement(text).Center();
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

            var stack = new VerticalStack(content, new Spacing(320, 0));
            AttachChild(new DialogFrame(stack) { Background = DialogFrameBackgroundStyle.DarkTint });
        }

        void Close()
        {
            Detach();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
    }
}
