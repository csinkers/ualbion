using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    class Header : UiElement
    {
        static readonly HandlerSet Handlers = null;
        public Header(StringId id) : base(Handlers) => AttachChild(new TextElement(id).NoWrap().Bold().Center());
        public Header(IText source) : base(Handlers) => AttachChild(new TextElement(source).NoWrap().Bold().Center());
    }
}
