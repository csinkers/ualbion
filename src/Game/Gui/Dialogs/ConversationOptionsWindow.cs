using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationOptionsWindow : ModalDialog
{
    const int MaxConversationOptionWidth = 283;
    readonly List<IUiElement> _optionElements = new();
    // Opaque background
    // Yellow options, gap, then white options.

    public ConversationOptionsWindow(int depth) : base(DialogPositioning.Bottom, depth)
    {
        On<RespondEvent>(e =>
        {
            int i = 1;
            foreach(var option in _optionElements)
            {
                if (option is not ConversationOption conversationOption)
                    continue;

                if (e.Option == i)
                    conversationOption.Trigger();
                i++;
            }
        });
    }

    public void SetOptions(IEnumerable<(IText, int?, Action)> options, IEnumerable<(IText, int?, Action)> standardOptions)
    {
        RemoveAllChildren();

        _optionElements.Clear();
        if (options != null)
            foreach (var (text, blockId, action) in options)
                _optionElements.Add(new ConversationOption(text, MaxConversationOptionWidth, blockId, action));

        if (standardOptions != null)
        {
            if (_optionElements.Any() && options?.Any() == true)
                _optionElements.Add(new Spacing(0, 10));

            foreach (var (text, blockId, action) in standardOptions)
                _optionElements.Add(new ConversationOption(text, MaxConversationOptionWidth, blockId, action));
        }

        _optionElements.Add(new Spacing(MaxConversationOptionWidth, 0));

        var stack = new VerticalStack(_optionElements);
        var content = new Padding(stack, 3);

        var frame = new DialogFrame(content);
        AttachChild(frame);
    }
}