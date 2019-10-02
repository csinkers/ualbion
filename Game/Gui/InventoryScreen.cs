using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    internal class InventoryScreen : IUiElement
    {
        class CharacterLeftPane
        {
            class Page1 : IUiElement // Summary
            {
                AlbionLabel _characterDescription; // Name, gender, age, race, class, level
                AlbionLabel _lblLifePoints;
                AlbionLabel _lblSpellPoints;
                AlbionLabel _lblExperiencePoints;
                AlbionLabel _lblTrainingPoints;
                public IUiElement Parent { get; }
                public IList<IUiElement> Children { get; }
                public Vector2 Size { get; }
                public bool FixedSize { get; }

                public void Render(Vector2 position, Action<IRenderable> addFunc)
                {
                    throw new NotImplementedException();
                }
            }

            class Page2 : IUiElement // Stats
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
                public IUiElement Parent { get; }
                public IList<IUiElement> Children { get; }
                public Vector2 Size { get; }
                public bool FixedSize { get; }

                public void Render(Vector2 position, Action<IRenderable> addFunc)
                {
                    throw new NotImplementedException();
                }
            }

            class Page3 : IUiElement
            {
                Header _conditionsHeader;
                AlbionLabel _conditions;
                Header _languagesHeader;
                AlbionLabel _languages;
                Header _temporarySpellsHeader;
                AlbionLabel _temporarySpells;

                Button _combatPositions;
                public IUiElement Parent { get; }
                public IList<IUiElement> Children { get; }
                public Vector2 Size { get; }
                public bool FixedSize { get; }

                public void Render(Vector2 position, Action<IRenderable> addFunc)
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

        class MidPane : IUiElement
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
            public IUiElement Parent { get; }
            public IList<IUiElement> Children { get; }
            public Vector2 Size { get; }
            public bool FixedSize { get; }

            public void Render(Vector2 position, Action<IRenderable> addFunc)
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
        public IUiElement Parent { get; }
        public IList<IUiElement> Children { get; }
        public Vector2 Size { get; }
        public bool FixedSize { get; }

        public void Render(Vector2 position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}
