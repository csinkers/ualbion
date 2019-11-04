using System.Numerics;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryMiscPage : UiElement
    {
        Header _conditionsHeader;
        Label _conditions;
        Header _languagesHeader;
        Label _languages;
        Header _temporarySpellsHeader;
        Label _temporarySpells;

        Button _combatPositions;
        public Vector2 Size { get; }
    }
}