using System;
using System.ComponentModel;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public enum InventoryMode
    {
        Character,
        Merchant,
        Chest,
        LockedDoor,
        LockedChest
    }

    public enum InventoryPage
    {
        Summary,
        Stats,
        Misc,
    }

    public class InventoryScreen : Dialog
    {
        const string ExitButtonId = "Inventory.Exit";
        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryScreen, SetInventoryModeEvent>((x, e) => { x._mode = e.Mode; x.Rebuild(); }),
            H<InventoryScreen, ButtonPressEvent>((x,e) => x.OnButton(e.ButtonId))
        );

        InventoryMode _mode;

        public InventoryScreen() : base(Handlers, DialogPositioning.Bottom) { }
        protected override void Subscribed()
        {
            Rebuild();
            base.Subscribed();
        }

        void OnButton(string buttonId)
        {
            var exchange = Exchange;
            switch (buttonId)
            {
                case ExitButtonId:
                    Raise(new PopSceneEvent());
                    break;
            }
        }

        void Rebuild()
        {
            foreach(var child in Children)
                child.Detach();
            Children.Clear();

            var background = new UiFixedPositionSprite<SlabId>(SlabId.SLAB, UiConstants.UiExtents);
            var leftPane =
                _mode switch
                {
                    InventoryMode.Character => (IUiElement)new InventoryCharacterPane(),
                    InventoryMode.Merchant => new InventoryChestPane(false),
                    InventoryMode.Chest => new InventoryChestPane(true),
                    InventoryMode.LockedChest => new InventoryLockPane(true),
                    InventoryMode.LockedDoor => new InventoryLockPane(false),
                    _ => throw new InvalidOperationException($"Unexpected inventory mode {_mode}")
                };

            var middlePane = new InventoryMidPane();
            var rightPane = new InventoryRightPane(ExitButtonId);
            // var frameDivider = new FrameDivider(135, 0, 4, 192);

            var leftContainer = new FixedPosition(
                new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane);

            var middleContainer = new FixedPosition(
                new Rectangle(141, 0, 135, UiConstants.ActiveAreaExtents.Height), middlePane);

            var rightContainer = new FixedPosition(
                new Rectangle(276, 0, 84, UiConstants.ActiveAreaExtents.Height), rightPane);

            Exchange
                .Attach(background)
                .Attach(leftContainer)
                .Attach(middlePane)
                .Attach(rightContainer)
                ;
            Children.Add(background);
            Children.Add(leftContainer);
            Children.Add(middleContainer);
            Children.Add(rightContainer);
        }
    }

    internal class InventoryLockPane : UiElement
    {
        public InventoryLockPane(bool isChest)
        {
        }
    }

    public class InventoryCharacterPane : UiElement
    {
        readonly InventorySummaryPage _summary;
        readonly InventoryStatsPage _stats;
        readonly InventoryMiscPage _misc;

        InventoryPage _page;

        public InventoryCharacterPane()
        {
            _summary = new InventorySummaryPage();
            _stats = new InventoryStatsPage();
            _misc = new InventoryMiscPage();
            Children.Add(_summary);
            Children.Add(_stats);
            Children.Add(_misc);
        }

        IUiElement GetActivePage() =>
            _page switch
            {
                InventoryPage.Summary => (IUiElement)_summary,
                InventoryPage.Stats => _stats,
                InventoryPage.Misc => _misc, 
                InventoryPage x => throw new NotImplementedException($"Unhandled inventory page \"{x}\"")
            };

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => GetActivePage().Render(extents, order, addFunc);
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc) => GetActivePage().Select(uiPosition, extents, order, registerHitFunc);
    }

    public class InventorySummaryPage : UiElement // Summary
    {
        Label _characterDescription; // Name, gender, age, race, class, level
        Label _lblLifePoints;
        Label _lblSpellPoints;
        Label _lblExperiencePoints;
        Label _lblTrainingPoints;
        public Vector2 Size { get; }
    }

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

    public class InventoryChestPane : UiElement
    {
        readonly bool _isChest;

        // readonly Header _chestHeader = new Header("Chest");
        InventorySlot[] _inventory = new InventorySlot[24]; // 6x4
        Button _money;
        Button _food;
        //TODO: Button _takeAll;

        public InventoryChestPane(bool isChest)
        {
            _isChest = isChest;
        }
    }

    public class InventoryMidPane : UiElement
    {
        Header _name;
        InventorySlot _head;
        InventorySlot _neck;
        InventorySlot _torso;
        InventorySlot _leftHand;
        InventorySlot _leftRing;
        InventorySlot _rightHand;
        InventorySlot _rightRing;
        InventorySlot _feet;
        InventorySlot _tail;

        Label _damage;
        Label _weight;
        Label _protection;
        public Vector2 Size { get; }
    }

    public class InventoryRightPane : UiElement
    {
        const int InventoryWidth = 4;
        const int InventoryHeight = 6;

        public InventoryRightPane(string exitButtonId)
        {
            var header = new Header(new StringId(AssetType.SystemText, 0, (int)SystemTextId.Inv_Backpack));
            var money = new ImageButton();
            var food = new ImageButton();
            var exit = new InventoryExitButton(exitButtonId);

            var slotSpans = new HorizontalStack[InventoryHeight];
            for (int j = 0; j < InventoryHeight; j++)
            {
                var slotsInRow = new IUiElement[InventoryWidth];
                for (int i = 0; i < InventoryWidth; i++)
                {
                    int index = j * InventoryWidth + i;
                    slotsInRow[i] = new InventorySlot(index);
                }
                slotSpans[j] = new HorizontalStack(slotsInRow);
            }

            var slotStack = new VerticalStack();
            var slotFrame = new ButtonFrame(slotStack);

            var stack = new VerticalStack(
                header,
                slotFrame,
                new HorizontalStack(money, food),
                exit);

            Children.Add(stack);
        }
    }

    public class ImageButton : UiElement
    {
    }
}
