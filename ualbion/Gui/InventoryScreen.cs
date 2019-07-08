using UAlbion.Entities;

namespace UAlbion.Gui
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
                AlbionHeader _attributes = new AlbionHeader("Attributes");
                AlbionIndicator _strength;
                AlbionIndicator _intelligence;
                AlbionIndicator _dexterity;
                AlbionIndicator _speed;
                AlbionIndicator _stamina;
                AlbionIndicator _luck;
                AlbionIndicator _magicResistance;
                AlbionIndicator _magicTalent;

                AlbionHeader _skills;
                AlbionIndicator _closeCombat;
                AlbionIndicator _rangedCombat;
                AlbionIndicator _criticalChance;
                AlbionIndicator _lockPicking;
            }

            class Page3 : GuiElement
            {
                AlbionHeader _conditionsHeader;
                AlbionLabel _conditions;
                AlbionHeader _languagesHeader;
                AlbionLabel _languages;
                AlbionHeader _temporarySpellsHeader;
                AlbionLabel _temporarySpells;

                AlbionButton _combatPositions;
            }
            AlbionButton _page1Button;
            AlbionButton _page2Button;
            AlbionButton _page3Button;
        }

        class ChestLeftPane
        {
            readonly AlbionHeader _chestHeader = new AlbionHeader("Chest");
            InventoryButton[] _inventory = new InventoryButton[24]; // 6x4
            AlbionButton _money;
            AlbionButton _food;
            //TODO: AlbionButton _takeAll;
        }

        class MidPane : GuiElement
        {
            AlbionHeader _name;
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
            AlbionHeader _backpack;
            AlbionButton _money;
            AlbionButton _food;
            AlbionButton _exit;
        }

        bool _isChest;
        Item _itemInHand;
    }
}
