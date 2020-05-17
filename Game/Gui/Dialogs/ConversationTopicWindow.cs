using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

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

        public void SetOptions(IEnumerable<WordId> words)
        {
            RemoveAllChildren();

            var elements = new List<IUiElement>();
            var wordButtons = words.Select(x =>
                (IUiElement)new Button(x.ToId(), () => OnWordSelected(x))
                {
                    Theme = ButtonTheme.DialogOption
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