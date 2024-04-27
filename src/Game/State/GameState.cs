﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.State;

public class GameState : GameServiceComponent<IGameState>, IGameState
{
    const int DaysPerMonth = 30;
    const int HoursPerDay = 24;

    readonly SheetApplier _sheetApplier;
    SavedGame _game;
    Party _party;
    CharacterSheet _leader;
    CharacterSheet _subject;
    CharacterSheet _currentInventory;
    CharacterSheet _combatant;
    CharacterSheet _victim;
    ItemData _weapon;

    public ICharacterSheet Leader    => _leader;
    public ICharacterSheet Subject   => _subject;
    public ICharacterSheet CurrentInventory => _currentInventory;
    public ICharacterSheet Combatant => _combatant;
    public ICharacterSheet Victim    => _victim;
    public ItemData Weapon => _weapon;

    public DateTime Time => SavedGame.Epoch + (_game?.ElapsedTime ?? TimeSpan.Zero);
    public IParty Party => _party;
    public ICharacterSheet GetSheet(SheetId id) => _game.Sheets.TryGetValue(id, out var sheet) ? sheet : null;
    public short GetTicker(TickerId id) => _game.Tickers.TryGetValue(id, out var value) ? value : (short)0;
    public bool GetSwitch(SwitchId id) => _game.GetSwitch(id);
    public IPlayer GetPlayerForCombatPosition(int position) => 
        _party.StatusBarOrder
            .Where((_, i) => _game.CombatPositions[i] == position)
            .FirstOrDefault();

    public int? GetCombatPositionForPlayer(PartyMemberId id)
    {
        var offset = (SavedGame.CombatRows - SavedGame.CombatRowsForParty) * SavedGame.CombatColumns;
        for (int i = 0; i < _party.StatusBarOrder.Count; i++)
            if (_party.StatusBarOrder[i].Id == id)
                return _game.CombatPositions[i] + offset;

        return null;
    }

    public MapChangeCollection TemporaryMapChanges => _game.TemporaryMapChanges;
    public MapChangeCollection PermanentMapChanges => _game.PermanentMapChanges;
    public ActiveItems ActiveItems => _game.ActiveItems;
    public IList<NpcState> Npcs => _game.Npcs;
    public bool IsChainDisabled(MapId mapId, ushort chain) => _game.IsChainDisabled(mapId, chain);
    public bool IsNpcDisabled(MapId mapId, byte npcNum) => _game.IsNpcDisabled(mapId, npcNum);
    public bool IsEventUsed(AssetId eventSetId, ActionEvent action) => _game.IsEventUsed(eventSetId, action);

    public MapId MapId => _game.MapId;
    public MapId MapIdForNpcs
    {
        get => _game.MapIdForNpcs;
        set => _game.MapIdForNpcs = value;
    }

#pragma warning disable CA1506 // '.ctor' is coupled with '42' different types from '11' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.
    public GameState()
    {
        OnAsync<NewGameEvent>(e => NewGame(e.MapId, e.X, e.Y));
        OnAsync<LoadGameEvent>(e => LoadGame(e.Id));
        On<SaveGameEvent>(e => SaveGame(e.Id, e.Name));
        On<FastClockEvent>(e => TickCount += e.Frames);
        On<GetTimeEvent>(_ => Info(Time.ToString("O")));
        On<SetTimeEvent>(e => _game.ElapsedTime = e.Time - SavedGame.Epoch);
        On<EventVisitedEvent>(e => _game?.UseEvent(e.Id, e.Action));
        On<LoadMapEvent>(OnLoadMap);
        On<SwitchEvent>(OnSwitch);
        On<TickerEvent>(OnTicker);
        On<ModifyDaysEvent>(OnModifyDays);
        On<ModifyHoursEvent>(OnModifyHours);
        On<ModifyMTicksEvent>(OnModifyMTicks);
        On<SetSpecialItemActiveEvent>(ActivateItem);
        On<EventChainOffEvent>(e => _game.SetChainDisabled(e.Map, e.ChainNumber, SetFlag(e.Operation, _game.IsChainDisabled(e.Map, e.ChainNumber))));
        On<ModifyNpcOffEvent>(e => _game.SetNpcDisabled(e.Map, e.NpcNum, SetFlag(e.Operation, _game.IsNpcDisabled(e.Map, e.NpcNum))));
        On<NpcOffEvent>(e => _game.SetNpcDisabled(MapId.None, e.NpcNum, true));
        On<NpcOnEvent>(e => _game.SetNpcDisabled(MapId.None, e.NpcNum, false));
        On<SetChestOpenEvent>(e => _game.SetChestOpen(e.Chest, SetFlag(e.Operation, _game.IsChestOpen(e.Chest))));
        On<SetDoorOpenEvent>(e => _game.SetDoorOpen(e.Door, SetFlag(e.Operation, _game.IsDoorOpen(e.Door))));
        On<DataChangeEvent>(OnDataChange);
        On<SetContextEvent>(OnSetContext);

        AttachChild(new InventoryManager(GetWriteableInventory, GetItem));
        _sheetApplier = AttachChild(new SheetApplier());
    }
#pragma warning restore CA1506 // '.ctor' is coupled with '42' different types from '11' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.

    void OnLoadMap(LoadMapEvent e)
    {
        if (_game != null)
            _game.MapId = e.MapId;
    }

    void OnSwitch(SwitchEvent e)
    {
        _game.SetSwitch(e.SwitchId, e.Operation switch
        {
            SwitchOperation.Clear => false,
            SwitchOperation.Set => true,
            SwitchOperation.Toggle => !_game.GetSwitch(e.SwitchId),
            _ => false
        });
    }

    void OnTicker(TickerEvent e)
    {
        _game.Tickers.TryGetValue(e.TickerId, out var curValue);
        _game.Tickers[e.TickerId] = (byte)e.Operation.Apply(curValue, e.Amount, 0, 255);
    }

    void OnModifyDays(ModifyDaysEvent e) // Only SetAmount + AddAmount
    {
        switch (e.Operation)
        {
            case NumericOperation.SetAmount:
            {
                var time = _game.ElapsedTime;
                var dayOfMonth = e.Amount % DaysPerMonth;
                int month = time.Days / DaysPerMonth;
                int newDays = month * DaysPerMonth + dayOfMonth;
                _game.ElapsedTime = new TimeSpan(newDays, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
                break;
            }
            case NumericOperation.AddAmount:
                for (int i = 0; i < e.Amount; i++) AdvanceTimeInHours(e.Amount * HoursPerDay);
                break;
        }
    }

    void OnModifyHours(ModifyHoursEvent e) // Only SetAmount + AddAmount
    {
        switch (e.Operation)
        {
            case NumericOperation.SetAmount:
            {
                var time = _game.ElapsedTime;
                _game.ElapsedTime = new TimeSpan(time.Days, e.Amount % HoursPerDay, time.Minutes, time.Seconds);
                break;
            }
            case NumericOperation.AddAmount:
                AdvanceTimeInHours(e.Amount);
                _game.ElapsedTime += TimeSpan.FromHours(e.Amount);
                break;
        }
    }

    void OnModifyMTicks(ModifyMTicksEvent e) // Only SetAmount + AddAmount
    {
        switch (e.Operation)
        {
            case NumericOperation.SetAmount:
            {
                var time = _game.ElapsedTime;
                var minutes = e.Amount * 60.0f / 48;
                _game.ElapsedTime = new TimeSpan(time.Days, time.Hours, 0, 0) + TimeSpan.FromMinutes(minutes);
                break;
            }
            case NumericOperation.AddAmount:
            {
                var minutes = e.Amount * 60.0f / 48;
                _game.ElapsedTime += TimeSpan.FromMinutes(minutes);
                // AdvanceTimeInTicks(e.Amount);
                break;
            }
        }
    }

    void OnSetContext(SetContextEvent e)
    {
        var state = Resolve<IGameState>();

        var asset = e.AssetId.Type switch
        {
            AssetType.PartyMember => (object)state.GetSheet(((PartyMemberId)e.AssetId).ToSheet()),
            AssetType.PartySheet => state.GetSheet(e.AssetId),
            AssetType.NpcSheet => state.GetSheet(e.AssetId),
            AssetType.MonsterSheet => Assets.LoadSheet(e.AssetId),
            AssetType.Item => Assets.LoadItem(e.AssetId),
            _ => null
        };

        switch (e.Type)
        {
            case ContextType.Leader:
                _leader = (CharacterSheet)asset;
                break;
            case ContextType.Subject:
                _subject = (CharacterSheet)asset;
                break;
            case ContextType.Inventory:
                _currentInventory = (CharacterSheet)asset;
                break;
            case ContextType.Combatant:
                _combatant = (CharacterSheet)asset;
                break;
            case ContextType.Victim:
                _victim = (CharacterSheet)asset;
                break;
            case ContextType.Weapon:
                _weapon = (ItemData)asset;
                break;
        }
    }

    void AdvanceTimeInHours(int hours)
    {
        _game.ElapsedTime += TimeSpan.FromHours(hours);
        // TODO
    }

    static bool SetFlag(SwitchOperation operation, bool value) =>
        operation switch
        {
            SwitchOperation.Clear => false,
            SwitchOperation.Set => true,
            SwitchOperation.Toggle => !value,
            _ => value
        };

    CharacterSheet GetTarget(TargetId id)
    {
        switch (id.Type)
        {
            case AssetType.PartyMember or AssetType.NpcSheet:
                return _game.Sheets.TryGetValue((AssetId)id, out var target) ? target : null;

            case AssetType.Target:
            {
                if (id == Base.Target.Leader) return _leader;
                if (id == Base.Target.Inventory) return _currentInventory;
                if (id == Base.Target.Attacker) return _combatant;
                if (id == Base.Target.Target) return _victim;
                if (id == Base.Target.Subject) return _subject;
                return null;

            }
            default: return null;
        }
    }

    void OnDataChange(IDataChangeEvent e)
    {
        if (e.Target == Base.Target.Everyone)
        {
            foreach (var member in _party.StatusBarOrder.Select(x => _game.Sheets[x.Id.ToSheet()]))
                _sheetApplier.Apply(e, member);
            return;
        }

        var target = GetTarget(e.Target);
        if (target == null)
        {
            Warn($"Could not resolve target {e.Target} when executing \"{e}\"");
            return;
        }

        _sheetApplier.Apply(e, target);
    }


    public IInventory GetInventory(InventoryId id)
    {
        if (id.Type == InventoryType.Player)
        {
            var player = _party[id.ToAssetId()];
            if (player != null)
                return player.Apparent.Inventory;
        }

        return GetWriteableInventory(id);
    }

    Inventory GetWriteableInventory(InventoryId id)
    {
        Inventory inventory;
        switch(id.Type)
        {
            case InventoryType.Player:
                inventory = _game.Sheets.TryGetValue(id.ToSheetId(), out var member) ? member.Inventory : null;
                break;
            case InventoryType.Chest: _game.Inventories.TryGetValue(id.ToAssetId(), out inventory); break;
            case InventoryType.Merchant: _game.Inventories.TryGetValue(id.ToAssetId(), out inventory); break;
            default:
                throw new InvalidOperationException($"Unexpected inventory type requested: \"{id.Type}\"");
        }

        return inventory;
    }

    ItemData GetItem(ItemId id) => Assets.LoadItemStrict(id);

    public int TickCount { get; private set; }
    public bool Loaded => _game != null;

    AlbionTask NewGame(MapId mapId, ushort x, ushort y)
    {
        Raise(new ReloadAssetsEvent()); // Make sure we don't end up with cached assets from the last game.
        _game = new SavedGame
        {
            MapId = mapId,
            ElapsedTime = TimeSpan.FromHours(8), // Game starts at 8:00 AM
            PartyX = x,
            PartyY = y,
            PartyDirection = Direction.East,
            ActiveMembers = { [0] = Base.PartyMember.Tom },
            CombatPositions = { [0] = 1 } // Tom starts off in the second position
        };

        var assets = Assets;
        foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.PartySheet))
            _game.Sheets.Add(id, assets.LoadSheet(id));

        foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.NpcSheet))
            _game.Sheets.Add(id, assets.LoadSheet(id));

        foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.Chest))
            _game.Inventories.Add(id, assets.LoadInventory(id));

        foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.Merchant))
            _game.Inventories.Add(id, assets.LoadInventory(id));

        return InitialiseGame();
    }

    string IdToPath(ushort id)
    {
        var pathResolver = Resolve<IPathResolver>();
        // TODO: This path currently exists in two places: here and Game\Gui\Menus\PickSaveSlot.cs
        return pathResolver.ResolvePath($"$(SAVES)/SAVE.{id:D3}");
    }

    AlbionTask LoadGame(ushort id)
    {
        _game = Assets.LoadSavedGame(IdToPath(id));
        if (_game == null)
            return AlbionTask.Complete;

        return InitialiseGame();
    }

    void SaveGame(ushort id, string name)
    {
        if (_game == null)
            return;

        var disk = Resolve<IFileSystem>();
        var spellManager = Resolve<ISpellManager>();
        _game.Name = name;

        for (int i = 0; i < SavedGame.MaxPartySize; i++)
            _game.ActiveMembers[i] = _party.StatusBarOrder.Count > i
                ? _party.StatusBarOrder[i].Id
                : PartyMemberId.None;

        // var key = new AssetId(AssetType.SavedGame, id);
        using var stream = disk.OpenWriteTruncate(IdToPath(id));
        using var bw = new BinaryWriter(stream);
        var mapping = new AssetMapping(); // TODO
        using var aw = new AlbionWriter(bw);
        SavedGame.Serdes(_game, mapping, aw, spellManager);
    }

    async AlbionTask InitialiseGame()
    {
        _party?.Remove();
        _party = AttachChild(new Party(_game.Sheets, GetWriteableInventory, _game.CombatPositions));

        foreach (var member in _game.ActiveMembers)
            if (!member.IsNone)
                _party.AddMember(member);

        await RaiseAsync(new LoadMapEvent(_game.MapId));

        Raise(new StartClockEvent());
        Raise(new SetPartyLeaderEvent(_party.Leader.Id, 0, 0));
        Raise(new PartyChangedEvent());
        Raise(new PartyJumpEvent(_game.PartyX, _game.PartyY));
        Raise(new CameraJumpEvent(_game.PartyX, _game.PartyY));
        Raise(new PartyTurnEvent(_game.PartyDirection));
        Raise(new PlayerEnteredTileEvent(_game.PartyX, _game.PartyY));
    }

    void ActivateItem(SetSpecialItemActiveEvent e)
    {
        if (e.IsActive)
        {
            if (e.Item == Base.Item.Clock) _game.ActiveItems |= ActiveItems.Clock;
            else if (e.Item == Base.Item.Compass) _game.ActiveItems |= ActiveItems.Compass;
            else if (e.Item == Base.Item.MonsterEye) _game.ActiveItems |= ActiveItems.MonsterEye;
        }
        else
        {
            if (e.Item == Base.Item.Clock) _game.ActiveItems &= ~ActiveItems.Clock;
            else if (e.Item == Base.Item.Compass) _game.ActiveItems &= ~ActiveItems.Compass;
            else if (e.Item == Base.Item.MonsterEye) _game.ActiveItems &= ~ActiveItems.MonsterEye;
        }
    }
}
