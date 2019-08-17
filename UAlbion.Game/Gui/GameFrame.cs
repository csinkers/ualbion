using System.Collections.Generic;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Gui
{
    public class CharacterPortrait : GuiElement
    {
    }

    public class GameFrame : GuiElement
    {
        enum GameState
        {
            Inventory,
            World2D,
            World3D
        }

        IList<CharacterPortrait> _characterPortraits = new List<CharacterPortrait>();
        AlbionLabel _notes;
        AlbionLabel _hovered;
        InventoryScreen _inventory;
        MapData2D _map2d;
        MapData3D _map3d;
    }
}
