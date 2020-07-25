using System;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationOption : UiElement
    {
        readonly Action _action;

        public ConversationOption(IText text, int? blockId, Action action)
        {
            _action = action;
            AttachChild(new Button(new UiText(text) { BlockFilter = blockId })
            {
                // Full width, invisible except hover (then white background w/ alpha blend)
                Theme = ButtonTheme.Frameless
            }.OnClick(action));
        }

        public void Trigger() => _action();
    }
}