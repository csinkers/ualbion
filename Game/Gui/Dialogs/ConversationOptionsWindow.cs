using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationOptionsWindow : ModalDialog
    {
        List<IUiElement> _optionElements;
        // Opaque background
        // Yellow options, gap, then white options.

        public ConversationOptionsWindow() : base(DialogPositioning.Bottom, 1)
        {
            On<RespondEvent>(e =>
            {
                int i = 1;
                foreach(var option in _optionElements.OfType<ConversationOption>())
                {
                    if (e.Option == i)
                        option.Trigger();
                    i++;
                }
            });
        }

        public void SetOptions(IEnumerable<(IText, int?, Action)> options, IEnumerable<(IText, int?, Action)> standardOptions)
        {
            RemoveAllChildren();

            _optionElements = new List<IUiElement>();
            if (options != null)
                foreach (var (text, blockId, action) in options)
                    _optionElements.Add(new ConversationOption(text, blockId, action));

            if (standardOptions != null)
            {
                if (_optionElements.Any() && options?.Any() == true)
                    _optionElements.Add(new Spacing(0, 10));

                foreach (var (text, blockId, action) in standardOptions)
                    _optionElements.Add(new ConversationOption(text, blockId, action));
            }

            _optionElements.Add(new Spacing(283, 0));

            var stack = new VerticalStack(_optionElements);
            var content = new Padding(stack, 3);

            var frame = new DialogFrame(content);
            AttachChild(frame);
        }
    }
}
