using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationOption : UiElement
    {
        public ConversationOption(IText text, int? blockId, Action action)
        {
            AttachChild(new Button(new TextElement(text) { BlockFilter = blockId }, action));
        }

        public ConversationOption(StringId text, Action action)
        {
            AttachChild(new Button(text, action));
        }

        // Full width, invisible except hover (then white background w/ alpha blend)
    }
}