using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    class DialogOption : Button
    {
        public DialogOption(string buttonId, ITextSource textSource) : base(buttonId,  new TextSection(textSource).Left().NoWrap())
        {
            Theme = new DialogOptionTheme();
        }
    }
}