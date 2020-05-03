using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    class Header : UiElement
    {
        public Header(StringId id) => AttachChild(new TextElement(id).NoWrap().Bold().Center());
        public Header(IText source) => AttachChild(new TextElement(source).NoWrap().Bold().Center());
    }
}
