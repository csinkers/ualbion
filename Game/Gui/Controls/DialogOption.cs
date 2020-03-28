using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    class DialogOption : Button
    {
        public DialogOption(string buttonId, IText textSource) : base(buttonId,  new TextBlockElement(textSource).Left().NoWrap())
        {
            Theme = new DialogOptionTheme();
        }
    }
}
