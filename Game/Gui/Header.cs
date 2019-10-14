using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    class Header : UiElement
    {
        static readonly Handler[] Handlers = { };

        public Header(StringId id) : base(Handlers)
        {
            var text = new Text(id).Bold().Center();
            Children.Add(text);
        }
    }
}