using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationOptionsWindow : Dialog
    {
        // Opaque background
        // Yellow options, gap, then white options.

        static readonly HandlerSet Handlers = new HandlerSet(
        );

        public ConversationOptionsWindow()
            : base(Handlers, DialogPositioning.Bottom, 1)
        {
        }

        public void SetOptions(IEnumerable<(IText, int?, Action)> options, IEnumerable<(IText, int?, Action)> standardOptions)
        {
            foreach(var child in Children)
                child.Detach();
            Children.Clear();

            var optionElements = new List<IUiElement>();
            foreach(var (text, blockId, action) in options)
                optionElements.Add(new ConversationOption(text, blockId, action));

            if (standardOptions?.Any() == true)
            {
                if (optionElements.Any())
                    optionElements.Add(new Spacing(0, 10));

                foreach (var (text, blockId, action) in standardOptions)
                    optionElements.Add(new ConversationOption(text, blockId, action));
            }

            optionElements.Add(new Spacing(283, 0));

            var stack = new VerticalStack(optionElements);
            var content = new Padding(stack, 3);

            var frame = new DialogFrame(content);
            AttachChild(frame);
        }
    }
}