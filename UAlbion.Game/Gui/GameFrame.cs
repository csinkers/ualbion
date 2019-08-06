using System.Collections.Generic;
using UAlbion.Formats;
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
        Map2D _map2d;
        Map3D _map3d;
    }
}
