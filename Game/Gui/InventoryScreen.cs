using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    internal class InventoryScreen : Component, IUiElement
    {
        public InventoryScreen() : base(null) { }
        /*
        class CharacterLeftPane
        {
            class Page1 : Component, IUiElement // Summary
            {
                public Page1() : base(null) { }
                Label _characterDescription; // Name, gender, age, race, class, level
                Label _lblLifePoints;
                Label _lblSpellPoints;
                Label _lblExperiencePoints;
                Label _lblTrainingPoints;
                public Vector2 Size { get; }

                public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
                {
                    throw new NotImplementedException();
                }
            }

            class Page2 : Component, IUiElement // Stats
            {
                public Page2() : base(null) { }
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

                public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
                {
                    throw new NotImplementedException();
                }
            }

            class Page3 : Component, IUiElement
            {
                public Page3() : base(null) { }
                Header _conditionsHeader;
                Label _conditions;
                Header _languagesHeader;
                Label _languages;
                Header _temporarySpellsHeader;
                Label _temporarySpells;

                Button _combatPositions;
                public Vector2 Size { get; }

                public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
                {
                    throw new NotImplementedException();
                }
            }
            Button _page1Button;
            Button _page2Button;
            Button _page3Button;
        }

        class ChestLeftPane
        {
            // readonly Header _chestHeader = new Header("Chest");
            InventoryButton[] _inventory = new InventoryButton[24]; // 6x4
            Button _money;
            Button _food;
            //TODO: Button _takeAll;
        }

        class MidPane : Component, IUiElement
        {
            public MidPane() : base(null) { }
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

            Label _damage;
            Label _weight;
            Label _protection;
            public Vector2 Size { get; }

            public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
            {
                throw new NotImplementedException();
            }
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
        */

        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}
