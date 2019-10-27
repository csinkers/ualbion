using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public enum InventoryMode
    {
        Merchant,
        Chest,
        Summary,
        Stats,
        Misc,
        LockedDoor,
        LockedChest
    }

    public class InventoryScreen : Dialog
    {
        static readonly HandlerSet Handlers = new HandlerSet(
        );

        InventoryMode _mode;

        public InventoryScreen() : base(Handlers, DialogPositioning.Bottom)
        {
            // 0 - 135 .375
            // 6 pix gap .0166667
            // 141 - 141+135 (276) .375
            // 276 - 276+84 (360)   .233333

            var background = new UiFixedPositionSprite<SlabId>(SlabId.SLAB, UiConstants.UiExtents);
            var leftPane = new InventoryLeftPane(() => _mode);
            var middlePane = new InventoryMidPane();
            var rightPane = new InventoryRightPane(() => _mode);
            // var frameDivider = new FrameDivider(135, 0, 4, 192);
            Children.Add(background);
            Children.Add(new FixedPosition(new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane));
            Children.Add(new FixedPosition(new Rectangle(141, 0, 135, UiConstants.ActiveAreaExtents.Height), middlePane));
            Children.Add(new FixedPosition(new Rectangle(276, 0, 84, UiConstants.ActiveAreaExtents.Height), rightPane));
        }
    }

    public class InventoryLeftPane : UiElement
    {
        readonly Func<InventoryMode> _modeGetter;
        readonly InventoryChestPage _chest;
        readonly InventoryMerchantPage _merchant;
        readonly InventorySummaryPage _summary;
        readonly InventoryStatsPage _stats;
        readonly InventoryMiscPage _misc;

        public InventoryLeftPane(Func<InventoryMode> modeGetter) : base(null)
        {
            _modeGetter = modeGetter;
            _chest = new InventoryChestPage();
            _merchant = new InventoryMerchantPage();
            _summary = new InventorySummaryPage();
            _stats = new InventoryStatsPage();
            _misc = new InventoryMiscPage();
            Children.Add(_chest);
            Children.Add(_merchant);
            Children.Add(_summary);
            Children.Add(_stats);
            Children.Add(_misc);
        }

        IUiElement GetActivePage() =>
            _modeGetter() switch
            {
                InventoryMode.Merchant => (IUiElement)_merchant,
                InventoryMode.Chest => _chest,
                InventoryMode.Summary => _summary,
                InventoryMode.Stats => _stats,
                InventoryMode.Misc => _misc, 
                InventoryMode x => throw new NotImplementedException($"Unhandled inventory page \"{x}\"")
            };

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => GetActivePage().Render(extents, order, addFunc);
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc) => GetActivePage().Select(uiPosition, extents, order, registerHitFunc);
    }

    public class InventorySummaryPage : UiElement // Summary
    {
        public InventorySummaryPage() : base(null) { }
        Label _characterDescription; // Name, gender, age, race, class, level
        Label _lblLifePoints;
        Label _lblSpellPoints;
        Label _lblExperiencePoints;
        Label _lblTrainingPoints;
        public Vector2 Size { get; }
    }

    public class InventoryStatsPage : UiElement // Stats
    {
        public InventoryStatsPage() : base(null) { }
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
        public InventoryMiscPage() : base(null) { }
        Header _conditionsHeader;
        Label _conditions;
        Header _languagesHeader;
        Label _languages;
        Header _temporarySpellsHeader;
        Label _temporarySpells;

        Button _combatPositions;
        public Vector2 Size { get; }
    }
    public class InventoryMerchantPage : UiElement
    {
        // readonly Header _chestHeader = new Header("Chest");
        InventorySlot[] _inventory = new InventorySlot[24]; // 6x4
        //TODO: Button _takeAll;
        public InventoryMerchantPage() : base(null) { }
    }

    public class InventoryChestPage : UiElement
    {
        // readonly Header _chestHeader = new Header("Chest");
        InventorySlot[] _inventory = new InventorySlot[24]; // 6x4
        Button _money;
        Button _food;
        //TODO: Button _takeAll;

        public InventoryChestPage() : base(null) { }
    }

    public class InventoryMidPane : UiElement
    {
        public InventoryMidPane() : base(null) { }
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
        readonly Func<InventoryMode> _modeGetter;
        readonly InventorySlot[] _inventory = new InventorySlot[24]; // 4x6
        Header _backpack;
        Button _money;
        Button _food;
        Button _exit;

        public InventoryRightPane(Func<InventoryMode> modeGetter) : base(null)
        {
            _modeGetter = modeGetter;
        }
    }
}
