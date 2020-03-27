using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    class DialogOption : Button
    {
        public DialogOption(string buttonId, IText textSource) : base(buttonId,  new TextBlockElement(textSource).Left().NoWrap())
        {
            Theme = new DialogOptionTheme();
        }
    }
}
