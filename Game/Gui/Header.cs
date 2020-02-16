using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    class Header : UiElement
    {
        static readonly HandlerSet Handlers = null;
        public Header(StringId id) : base(Handlers) => AttachChild(new Text(id).NoWrap().Bold().Center());
        public Header(ITextSource source) : base(Handlers) => AttachChild(new Text(source).NoWrap().Bold().Center());
    }
}
