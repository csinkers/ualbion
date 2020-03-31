using System;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationTextWindow : Dialog
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ConversationTextWindow, UiLeftClickEvent>((x, e) => x.Clicked?.Invoke())
        );

        readonly TextSourceWrapper _text = new TextSourceWrapper();

        public event Action Clicked;

        public ConversationTextWindow() : base(Handlers, DialogPositioning.Bottom)
        {
            var content = new FixedSize(248, 159,
                new Padding(
                    new TextElement(_text) { BlockFilter = 0 },
                    3));

            // Transparent background, scrollable
            var frame = new DialogFrame(content) { Background = DialogFrameBackgroundStyle.DarkTint };
            AttachChild(frame);
        }

        public IText Text
        {
            get => _text.Source;
            set => _text.Source = value;
        }
    }
}