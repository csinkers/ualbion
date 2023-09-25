using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Combat;

public class CombatManager : Component
{
    public CombatManager()
    {
        On<EncounterEvent>(e => BeginCombat(e.GroupId, e.BackgroundId));
    }

    void BeginCombat(MonsterGroupId groupId, SpriteId backgroundId)
    {
        Raise(new PushSceneEvent(SceneId.Combat));
        var battle = AttachChild(new Battle(groupId, backgroundId));
        battle.Complete += () =>
        {
            RemoveChild(battle);
            Raise(new PopSceneEvent());
        };
    }
}

[Scene(SceneId.Combat)]
public class CombatScene : Container, IScene
{
    bool _clockWasRunning;
    public ICamera Camera { get; }
    public CombatScene() : base(nameof(SceneId.Combat))
    {
        Camera = AttachChild(new PerspectiveCamera());
    }

    protected override void Subscribed()
    {
        _clockWasRunning = Resolve<IClock>().IsRunning;
        if (_clockWasRunning)
            Raise(new StopClockEvent());

        Raise(new PushMouseModeEvent(MouseMode.Normal));
        Raise(new PushInputModeEvent(InputMode.Combat));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopMouseModeEvent());
        Raise(new PopInputModeEvent());
        if (_clockWasRunning)
            Raise(new StartClockEvent());
    }
}

public interface IReadOnlyBattle
{
    IReadOnlyList<IReadOnlyMob> Mobs { get; }
    IReadOnlyMob GetTile(int x, int y);
}

public class Battle : Component, IReadOnlyBattle
{
    readonly List<Mob> _mobs = new();
    readonly Mob[] _tiles = new Mob[SavedGame.CombatRows * SavedGame.CombatColumns];
    readonly Sprite _background;
    readonly CombatDialog _dialog;

    public IReadOnlyList<IReadOnlyMob> Mobs { get; }
    public event Action Complete;

    public IReadOnlyMob GetTile(int x, int y)
    {
        int index = x + y * SavedGame.CombatColumns;
        return index < 0 || index >= _tiles.Length ? null : _tiles[index];
    }

    public Battle(MonsterGroupId groupId, SpriteId backgroundId)
    {
        Mobs = _mobs;
        _background = 
            AttachChild(new Sprite(
                backgroundId,
                DrawLayer.Interface,
                SpriteKeyFlags.NoTransform,
                SpriteFlags.LeftAligned)
            {
                Position = new Vector3(-1.0f, 1.0f, 0),
                Size = new Vector2(2.0f, -2.0f)
            });

        _dialog = AttachChild(new CombatDialog());
    }
}

public class CombatDialog : Dialog
{
    public CombatDialog() : base(DialogPositioning.Center, 0)
    {
    }
}

public class LogicalCombatSlot : UiElement
{
    readonly InventorySlotId _id;
    readonly VisualCombatSlot _visual;
    int _version = 1;

    public LogicalCombatSlot(InventorySlotId id)
    {
        On<InventoryChangedEvent>(e =>
        {
            if (e.Id == _id.Id)
                _version++;
        });

        _id = id;

        IText amountSource;
        if (id.Slot == ItemSlotId.Gold)
        {
            amountSource = new DynamicText(() =>
            {
                var gold = Inventory?.Gold.Amount ?? 0;
                return new[] { new TextBlock($"{gold / 10}.{gold % 10}") }; // todo: i18n: May need to vary based on the current game language
            }, _ => _version);
        }
        else if (id.Slot == ItemSlotId.Rations)
        {
            amountSource = new DynamicText(() =>
            {
                var food = Inventory?.Rations.Amount ?? 0;
                return new[] { new TextBlock(food.ToString()) }; // todo: i18n: Will need to be changed if we support a language that doesn't use Hindu-Arabic numerals.
            }, _ => _version);
        }
        else
        {
            amountSource = new DynamicText(() =>
            {
                var slotInfo = Slot;
                return slotInfo == null || slotInfo.Amount < 2
                    ? Array.Empty<TextBlock>()
                    : new[] { new TextBlock(slotInfo.Amount.ToString()) { Alignment = TextAlignment.Right } }; // todo: i18n: Will need to be changed if we support a language that doesn't use Hindu-Arabic numerals.
            }, _ => _version);
        }

        _visual = AttachChild(new VisualCombatSlot(_id, amountSource, () => Slot))
            .OnButtonDown(() =>
            {
                var im = Resolve<IInventoryManager>();
                var inputBinder = Resolve<IInputBinder>();
                if (!im.ItemInHand.Item.IsNone
                    || inputBinder.IsCtrlPressed
                    || inputBinder.IsShiftPressed
                    || inputBinder.IsAltPressed)
                {
                    _visual.SuppressNextDoubleClick = true;
                }
            })
            .OnClick(() =>
            {
                var inputBinder = Resolve<IInputBinder>();
                if (inputBinder.IsCtrlPressed)
                    Raise(new InventoryPickupEvent(null, _id.Id, _id.Slot));
                else if (inputBinder.IsShiftPressed)
                    Raise(new InventoryPickupEvent(5, _id.Id, _id.Slot));
                else if (inputBinder.IsAltPressed)
                    Raise(new InventoryPickupEvent(1, _id.Id, _id.Slot));
                else
                    Raise(new InventorySwapEvent(_id.Id, _id.Slot));
            })
            .OnDoubleClick(() => Raise(new InventoryPickupEvent(null, _id.Id, _id.Slot)))
            .OnRightClick(OnRightClick)
            .OnHover(Hover)
            .OnBlur(Blur);
    }

    public override string ToString() => $"InventorySlot:{_id}";
    IInventory Inventory => Resolve<IGameState>().GetInventory(_id.Id);
    IReadOnlyItemSlot Slot => Inventory?.GetSlot(_id.Slot);

    void Blur()
    {
        var inventoryManager = Resolve<IInventoryManager>();
        var hand = inventoryManager.ItemInHand;
        Raise(new SetCursorEvent(hand.Item.IsNone ? Base.CoreGfx.Cursor : Base.CoreGfx.CursorSmall));
        Raise(new HoverTextEvent(null));
    }

    void Hover()
    {
        var assets = Resolve<IAssetManager>();
        var inventoryManager = Resolve<IInventoryManager>();
        var inventory = Resolve<IGameState>().GetInventory(_id.Id);
        var tf = Resolve<ITextFormatter>();

        var slotInfo = Slot;
        string itemName = null;
        if (slotInfo?.Item.Type == AssetType.Item)
        {
            var item = assets.LoadItem(slotInfo.Item);
            itemName = assets.LoadString(item.Name);
        }

        var hand = inventoryManager.ItemInHand;
        string itemInHandName = null;
        if (hand.Item.Type == AssetType.Item)
        {
            var itemInHand = assets.LoadItem(hand.Item);
            itemInHandName = assets.LoadString(itemInHand.Name);
        }

        var action = inventoryManager.GetInventoryAction(_id);
        _visual.Hoverable = true;
        switch (action)
        {
            case InventoryAction.Nothing:
                _visual.Hoverable = false;
                break;
            case InventoryAction.Pickup: // <Item name>
            {
                if (itemName != null)
                {
                    Raise(new HoverTextEvent(new LiteralText(itemName)));
                    Raise(new SetCursorEvent(Base.CoreGfx.CursorSelected));
                }
                else if(_id.Slot is ItemSlotId.Gold or ItemSlotId.Rations)
                {
                    bool isGold = _id.Slot == ItemSlotId.Gold;
                    int amount = isGold ? inventory.Gold.Amount : inventory.Rations.Amount;
                    var text = isGold
                        ? tf.Format(Base.SystemText.Gold_NNGold, amount / 10, amount % 10)
                        : tf.Format(Base.SystemText.Gold_NRations, amount);
                    Raise(new HoverTextEvent(text));
                    Raise(new SetCursorEvent(Base.CoreGfx.CursorSelected));
                }
                break;
            }
            case InventoryAction.PutDown: // Put down %s
            {
                if (itemInHandName != null)
                {
                    var text = tf.Format(Base.SystemText.Item_PutDownX, itemInHandName);
                    Raise(new HoverTextEvent(text));
                }
                break;
            }
            case InventoryAction.Swap: // Swap %s with %s
            {
                if (itemInHandName != null && itemName != null)
                {
                    var text = tf.Format(Base.SystemText.Item_SwapXWithX, itemInHandName, itemName);
                    Raise(new HoverTextEvent(text));
                }
                break;
            }
            case InventoryAction.Coalesce: // Add
            {
                Raise(new HoverTextEvent(tf.Format(Base.SystemText.Item_Add)));
                break;
            }
            case InventoryAction.NoCoalesceFullStack: // {YELLOW}This space is occupied!
            {
                Raise(new HoverTextEvent(tf.Format(Base.SystemText.Item_ThisSpaceIsOccupied)));
                break;
            }
        }
    }

    void OnRightClick()
    {
        var inventory = Resolve<IGameState>().GetInventory(_id.Id);
        var slotInfo = inventory.GetSlot(_id.Slot);
        if (slotInfo?.Item.Type != AssetType.Item)
        {
            OnRightClickSpecial(slotInfo);
            return;
        }

        var tf = Resolve<ITextFormatter>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();

        var item = Resolve<IAssetManager>().LoadItem(slotInfo.Item);
        var itemPosition = window.UiToNorm(slotInfo.LastUiPosition);
        var heading = tf.Center().NoWrap().Fat().Format(item.Name);

        IText S(TextId textId, bool disabled = false)
            => tf
                .Center()
                .NoWrap()
                .Ink(disabled ? Base.Ink.Yellow : Base.Ink.White)
                .Format(textId);

        // Drop (Yellow inactive when critical)
        // Examine
        // Use (e.g. torch)
        // Drink
        // Activate (compass, clock, monster eye)
        // Activate spell (if has spell, yellow if combat spell & not in combat etc)
        // Read (e.g. metal-magic knowledge, maps)

        bool isPlotItem = (item.Flags & ItemFlags.PlotItem) != 0;
        var options = new List<ContextMenuOption>();

        if (_id.Id.Type == InventoryType.Merchant)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Sell, isPlotItem),
                isPlotItem 
                    ? new HoverTextEvent(
                        tf.Format(
                            Base.SystemText.InvMsg_ThisIsAVitalItem))
                    : new InventorySellEvent(_id.Id, _id.Slot),
                ContextMenuGroup.Actions,
                isPlotItem));
        }
        else
        {
            options.Add(
                new ContextMenuOption(
                    S(Base.SystemText.InvPopup_Drop, isPlotItem),
                    isPlotItem
                        ? new HoverTextEvent(
                            tf.Format(
                                Base.SystemText.InvMsg_ThisIsAVitalItem))
                        : new InventoryDiscardEvent(itemPosition.X, itemPosition.Y, _id.Id, _id.Slot),
                    ContextMenuGroup.Actions,
                    isPlotItem));
        }

        options.Add(new ContextMenuOption(
            S(Base.SystemText.InvPopup_Examine),
            new InventoryExamineEvent(item.Id),
            ContextMenuGroup.Actions));

        if (item.TypeId == ItemType.Document && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Read),
                new ReadItemEvent(_id),
                ContextMenuGroup.Actions));
        }

        if (item.TypeId == ItemType.SpellScroll && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_LearnSpell),
                new ReadSpellScrollEvent(_id),
                ContextMenuGroup.Actions));
        }

        if (item.TypeId == ItemType.Drink && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Drink),
                new DrinkItemEvent(_id),
                ContextMenuGroup.Actions));
        }

        if (item.TypeId == ItemType.HeadsUpDisplayItem && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Activate),
                new ActivateItemEvent(_id),
                ContextMenuGroup.Actions));
        }

        // TODO: Disable based on spell context
        if (item.Charges > 0 && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_ActivateSpell),
                new ActivateItemSpellEvent(_id),
                ContextMenuGroup.Actions));
        }

        var uiPosition = window.PixelToUi(cursorManager.Position);
        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }

    void OnRightClickSpecial(IReadOnlyItemSlot slotInfo)
    {
        if (slotInfo.Item.IsNone)
            return;

        var tf = Resolve<ITextFormatter>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();
        var headingText = slotInfo.Item == AssetId.Gold
            ? Base.SystemText.Gold_Gold 
            : Base.SystemText.Gold_Rations;

        var itemPosition = window.UiToNorm(slotInfo.LastUiPosition);
        var heading = tf.Center().NoWrap().Fat().Format(headingText);

        IText S(TextId textId) => tf.Center().NoWrap().Ink(Base.Ink.White).Format(textId);
        var options = new List<ContextMenuOption>
        {
            new(S( Base.SystemText.Gold_ThrowAway),
                new InventoryDiscardEvent(itemPosition.X, itemPosition.Y, _id.Id, _id.Slot),
                ContextMenuGroup.Actions)
        };

        var uiPosition = window.PixelToUi(cursorManager.Position);
        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }
}

public class VisualCombatTile : UiElement
{
    readonly UiSpriteElement _sprite;
    readonly UiSpriteElement _overlay;
    readonly Func<IReadOnlyItemSlot> _getSlot;
    readonly Button _button;
    readonly Vector2 _size;
    readonly int _index;

    int _frameNumber;

    public VisualCombatTile(int index, IText amountSource, Func<IReadOnlyItemSlot> getSlot)
    {
        On<IdleClockEvent>(_ => _frameNumber++);

        _index = index;
        _getSlot = getSlot;
        _overlay = new UiSpriteElement(Base.CoreGfx.UiBroken) { IsActive = false };

        _sprite = new UiSpriteElement(
            slotId.Slot == ItemSlotId.Gold
                ? Base.CoreGfx.UiGold
                : Base.CoreGfx.UiFood);

        _button = AttachChild(new Button(
                new VerticalStacker(
                        new Spacing(31, 0),
                        _sprite,
                        new UiText(amountSource)
                    )
                    { Greedy = false })
            .OnHover(() => Hover?.Invoke())
            .OnBlur(() => Blur?.Invoke())
            .OnClick(() => Click?.Invoke())
            .OnRightClick(() => RightClick?.Invoke())
            .OnDoubleClick(() => DoubleClick?.Invoke())
            .OnButtonDown(() => ButtonDown?.Invoke()));
    }

    // public ButtonState State { get => _frame.State; set => _frame.State = value; }
    public override Vector2 GetSize() => _sprite.Id.Type == AssetType.CoreGfx ? base.GetSize() : _size;
    public VisualInventorySlot OnClick(Action callback) { Click += callback; return this; }
    public VisualInventorySlot OnRightClick(Action callback) { RightClick += callback; return this; }
    public VisualInventorySlot OnDoubleClick(Action callback) { DoubleClick += callback; return this; }
    public VisualInventorySlot OnButtonDown(Action callback) { ButtonDown += callback; return this; }
    public VisualInventorySlot OnHover(Action callback) { Hover += callback; return this; }
    public VisualInventorySlot OnBlur(Action callback) { Blur += callback; return this; }
    event Action Click;
    event Action DoubleClick;
    event Action RightClick;
    event Action ButtonDown;
    event Action Hover;
    event Action Blur;

    public bool Hoverable { get => _button.Hoverable; set => _button.Hoverable = value; }
    public bool SuppressNextDoubleClick { get => _button.SuppressNextDoubleClick; set => _button.SuppressNextDoubleClick = value; }

    void Rebuild(in Rectangle extents)
    {
        var slot = _getSlot();
        if (slot == null)
            return;

        _button.AllowDoubleClick = slot.Amount > 1;

        if ((int)slot.LastUiPosition.X != extents.X || (int)slot.LastUiPosition.Y != extents.Y)
            Raise(new SetInventorySlotUiPositionEvent(_index, extents.X, extents.Y));

        if (slot.Item.Type == AssetType.Item)
        {
            var item = Resolve<IAssetManager>().LoadItem(slot.Item);
            int frames = item.IconAnim == 0 ? 1 : item.IconAnim;
            while (_frameNumber >= frames)
                _frameNumber -= frames;

            int itemSpriteId = item.IconSubId + _frameNumber;
            _sprite.Id = item.Icon;
            _sprite.SubId = itemSpriteId;
            _overlay.IsActive = (slot.Flags & ItemSlotFlags.Broken) != 0;
        }
        else // Special slots (i.e. rations + gold) keep their sprite when empty.
        {
            _sprite.Id = AssetId.None; // Nothing
            _sprite.SubId = 0;
            _overlay.IsActive = false;
        }
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        Rebuild(extents);
        return base.Render(extents, order, parent);
    }
}

public interface IReadOnlyMob
{
    public int X { get; }
    public int Y { get; }
    public ICharacterSheet Sheet { get; }
}

public class Mob : Component, IReadOnlyMob // Logical mob / character in a battle
{
    public Mob(ICharacterSheet sheet) => Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));

    public int X { get; private set; }
    public int Y { get; private set; }
    public ICharacterSheet Sheet { get; }
}

public class Mob3D : Component // Physical 3D/sprite representation of mob
{
}

public class Mob2D : Component // Physical representation of mob on combat planning dialog
{
}

public class SelectSpellDialog : Dialog
{
    public SelectSpellDialog(int depth) : base(DialogPositioning.Center, depth)
    {
    }
}

public class MobController : Component
{
}

public static class DamageCalculator
{
}


[Event("combat_update", "Run the combat clock for the specified number of slow-clock cycles")]
public class CombatUpdateEvent : Event, IAsyncEvent
{
    public CombatUpdateEvent(int cycles) => Cycles = cycles;

    [EventPart("cycles", "The number of slow-clock cycles to update for")]
    public int Cycles { get; }
}

[Event("combat_clock")] public class CombatClockEvent : GameEvent, IVerboseEvent { }
[Event("start_combat_clock", "Resume automatically updating the combat clock.")]
public class StartCombatClockEvent : GameEvent { }

[Event("stop_combat_clock", "Stop the combat clock from advancing automatically.")]
public class StopCombatClockEvent : GameEvent { }

public class CombatClock : Component
{
    readonly CombatClockEvent _event = new();
    int _ticks;
    int _combatTicks;

    float _elapsedTimeThisGameFrame;
    int _ticksRemaining;
    int _stoppedFrames;
    int _totalFastTicks;
    float _stoppedMs;
    Action _pendingContinuation;
    bool _isRunning;

    public CombatClock()
    {
        On<StartCombatClockEvent>(_ => IsRunning = true);
        On<StopCombatClockEvent>(_ => IsRunning = false);
        On<EngineUpdateEvent>(OnEngineUpdate);

        OnAsync<CombatUpdateEvent>((e, c) =>
        {
            if (IsRunning || _pendingContinuation != null)
                return false;

            GameTrace.Log.CombatClockUpdating(e.Cycles);
            _pendingContinuation = c;
            _ticksRemaining = e.Cycles * Var(GameVars.Time.FastTicksPerSlowTick);
            IsRunning = true;
            return true;
        });
    }

    void OnUpdate(FastClockEvent updateEvent)
    {
        _ticks += updateEvent.Frames;
        var ticksPerCombat = Var(GameVars.Time.FastTicksPerSlowTick);
        while (_ticks >= ticksPerCombat)
        {
            _ticks -= ticksPerCombat;
            GameTrace.Log.CombatTick(_combatTicks++);
            Raise(_event);
        }
    }

    public float ElapsedTime { get; private set; }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
                return;

            if (value)
            {
                GameTrace.Log.CombatClockStart(_stoppedFrames, _stoppedMs);
                _stoppedFrames = 0;
                _stoppedMs = 0;
            }
            else GameTrace.Log.CombatClockStop();

            _isRunning = value;
        }
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        ElapsedTime += e.DeltaSeconds;

        if (IsRunning)
        {
            _elapsedTimeThisGameFrame += e.DeltaSeconds;
            var tickDurationSeconds = 1.0f / Var(GameVars.Time.FastTicksPerSecond);

            // If the game was paused for a while don't try and catch up
            if (_elapsedTimeThisGameFrame > 4 * tickDurationSeconds)
                _elapsedTimeThisGameFrame = 4 * tickDurationSeconds;

            while (_elapsedTimeThisGameFrame >= tickDurationSeconds && IsRunning)
            {
                _elapsedTimeThisGameFrame -= tickDurationSeconds;
                RaiseTick();
            }
        }
        else
        {
            _stoppedFrames++;
            _stoppedMs += 1000.0f * e.DeltaSeconds;
        }
    }

    void RaiseTick()
    {
        GameTrace.Log.FastTick(_totalFastTicks++);
        Raise(new FastClockEvent(1));
        if (_ticksRemaining <= 0)
            return;

        _ticksRemaining --;
        if (_ticksRemaining > 0)
            return;

        IsRunning = false;
        GameTrace.Log.ClockUpdateComplete();
        var continuation = _pendingContinuation;
        _pendingContinuation = null;
        continuation?.Invoke();
    }
}