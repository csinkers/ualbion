using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    class Header : UiElement
    {
        static readonly HandlerSet Handlers = null;
        public Header(StringId id) : base(Handlers) => AttachChild(new TextBlockElement(id).NoWrap().Bold().Center());
        public Header(IText source) : base(Handlers) => AttachChild(new TextBlockElement(source).NoWrap().Bold().Center());
    }
}
