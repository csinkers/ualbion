﻿using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
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

    public AlbionTask<T> GetOption<T>((IText, BlockId?, T)[] options, (IText, BlockId?, T)[] standardOptions)
    {
        RemoveAllChildren();

        _optionElements.Clear();

        var source = new AlbionTaskSource<T>();
        void AddOption(IText text, BlockId? blockId, T result)
        {
            _optionElements.Add(
                new ConversationOption(
                    text,
                    MaxConversationOptionWidth,
                    blockId,
                    () =>
                    {
                        IsActive = false;
                        source.Complete(result);
                    }));
        }

        if (options != null)
            foreach (var (text, blockId, result) in options)
                AddOption(text, blockId, result);

        if (standardOptions != null)
        {
            if (_optionElements.Any() && options?.Any() == true)
                _optionElements.Add(new Spacing(0, 10));

            foreach (var (text, blockId, result) in standardOptions)
                AddOption(text, blockId, result);
        }

        _optionElements.Add(new Spacing(MaxConversationOptionWidth, 0));

        var stack = new VerticalStacker(_optionElements);
        var content = new Padding(stack, 3);

        var frame = new DialogFrame(content);
        AttachChild(frame);
        IsActive = true;

        return source.Task;
    }
}