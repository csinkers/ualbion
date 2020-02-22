using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    class Label : UiElement
    {
        public Label(StringId stringId) : base(null) => AttachChild(new TextSection(stringId).Center());
    }
}
