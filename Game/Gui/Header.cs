using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    class Header : UiElement
    {
        static readonly HandlerSet Handlers = null;

        public Header(StringId id) : base(Handlers)
        {
            var text = new Text(id).Bold().Center();
            Children.Add(text);
        }
        public Header(ITextSource source) : base(Handlers)
        {
            var text = new Text(source).Bold().Center();
            Children.Add(text);
        }
    }
}