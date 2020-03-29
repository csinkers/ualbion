using System;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    class DialogOption : Button
    {
        public DialogOption(IText textSource, Action action) 
            : base(new TextElement(textSource).Left().NoWrap(), action)
        {
            Theme = new DialogOptionTheme();
        }
    }
}
