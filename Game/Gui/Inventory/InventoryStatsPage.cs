using System.Numerics;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryStatsPage : UiElement // Stats
    {
        // Header _attributes = new Header("Attributes");
        AlbionIndicator _strength;
        AlbionIndicator _intelligence;
        AlbionIndicator _dexterity;
        AlbionIndicator _speed;
        AlbionIndicator _stamina;
        AlbionIndicator _luck;
        AlbionIndicator _magicResistance;
        AlbionIndicator _magicTalent;

        Header _skills;
        AlbionIndicator _closeCombat;
        AlbionIndicator _rangedCombat;
        AlbionIndicator _criticalChance;
        AlbionIndicator _lockPicking;
        public Vector2 Size { get; }
    }
}