using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    internal class InventoryScreen : GuiElement
    {
        class CharacterLeftPane
        {
            class Page1 : GuiElement // Summary
            {
                AlbionLabel _characterDescription; // Name, gender, age, race, class, level
                AlbionLabel _lblLifePoints;
                AlbionLabel _lblSpellPoints;
                AlbionLabel _lblExperiencePoints;
                AlbionLabel _lblTrainingPoints;
            }

            class Page2 : GuiElement // Stats
            {
                Header _attributes = new Header("Attributes");
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
            }

            class Page3 : GuiElement
            {
                Header _conditionsHeader;
                AlbionLabel _conditions;
                Header _languagesHeader;
                AlbionLabel _languages;
                Header _temporarySpellsHeader;
                AlbionLabel _temporarySpells;

                Button _combatPositions;
            }
            Button _page1Button;
            Button _page2Button;
            Button _page3Button;
        }

        class ChestLeftPane
        {
            readonly Header _chestHeader = new Header("Chest");
            InventoryButton[] _inventory = new InventoryButton[24]; // 6x4
            Button _money;
            Button _food;
            //TODO: Button _takeAll;
        }

        class MidPane : GuiElement
        {
            Header _name;
            InventoryButton _head;
            InventoryButton _neck;
            InventoryButton _torso;
            InventoryButton _leftHand;
            InventoryButton _leftRing;
            InventoryButton _rightHand;
            InventoryButton _rightRing;
            InventoryButton _feet;
            InventoryButton _tail;

            AlbionLabel _damage;
            AlbionLabel _weight;
            AlbionLabel _protection;
        }

        class RightPane
        {
            readonly InventoryButton[] _inventory = new InventoryButton[24]; // 4x6
            Header _backpack;
            Button _money;
            Button _food;
            Button _exit;
        }

        bool _isChest;
        Item _itemInHand;
    }
}
