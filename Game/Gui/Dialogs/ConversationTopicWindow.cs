using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationTopicWindow : ModalDialog
    {
        public ConversationTopicWindow() : base(DialogPositioning.Center, 2)
        {
            On<UiRightClickEvent>(e =>
            {
                e.Propagating = false;
                OnWordSelected(null);
            });
        }

        public event EventHandler<WordId?> WordSelected;
        void OnWordSelected(WordId? e) => WordSelected?.Invoke(this, e);

        public void SetOptions(IDictionary<WordId, WordStatus> words)
        {
            RemoveAllChildren();

            var elements = new List<IUiElement>();
            var wordButtons = words.Select(x =>
                {
                    var color = x.Value switch
                    {
                        WordStatus.Mentioned => FontColor.Yellow,
                        WordStatus.Discussed => FontColor.White,
                        _ => FontColor.Gray,
                    };

                    var textElement = (IUiElement)new UiTextBuilder(x.Key.ToId()).Ink(color);
                    return (IUiElement)new Button(textElement, () => OnWordSelected(x.Key))
                    {
                        Theme = ButtonTheme.Frameless
                    };
                }
            ).ToArray();

            if (wordButtons.Any())
            {
                elements.Add(new GroupingFrame(new VerticalStack(wordButtons)));
                elements.Add(new Spacing(0, 3));
            }

            elements.Add(new Button(SystemTextId.MsgBox_EnterWord.ToId(), () =>
            {
                // TODO
                OnWordSelected(null);
            }));

            AttachChild(new DialogFrame(new Padding(new VerticalStack(elements), 3))
            {
                Background = DialogFrameBackgroundStyle.MainMenuPattern
            });
        }
    }
}