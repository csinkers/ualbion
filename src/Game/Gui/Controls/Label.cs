using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Controls;

class Label : UiElement
{
    public Label(TextId textId) : this(new StringId(textId)) { }
    public Label(StringId stringId) => AttachChild(new UiTextBuilder(stringId).Center());
}