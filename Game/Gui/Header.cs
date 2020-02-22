using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    class Header : UiElement
    {
        static readonly HandlerSet Handlers = null;
        public Header(StringId id) : base(Handlers) => AttachChild(new TextSection(id).NoWrap().Bold().Center());
        public Header(ITextSource source) : base(Handlers) => AttachChild(new TextSection(source).NoWrap().Bold().Center());
    }
}
