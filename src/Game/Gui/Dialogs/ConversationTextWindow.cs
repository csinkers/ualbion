using System;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationTextWindow : ModalDialog
    {
        readonly TextSourceWrapper _text = new();

        public event Action Clicked;

        public ConversationTextWindow() : base(DialogPositioning.Bottom)
        {
            On<UiLeftClickEvent>(e => Clicked?.Invoke());
            On<DismissMessageEvent>(e => Clicked?.Invoke());

            var content = new FixedSize(248, 159,
                new Padding(
                    new UiText(_text)
                        .Scrollable()
                        .Filter(0),
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