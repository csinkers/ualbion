using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.State
{
    public class GameState : ServiceComponent<IGameState>, IGameState
    {
        SavedGame _game;
        Party _party;

        public DateTime Time => SavedGame.Epoch + (_game?.ElapsedTime ?? TimeSpan.Zero);
        public float PaletteBlend => MathF.Cos((float)Time.TimeOfDay.TotalDays * MathF.PI * 2 * 60) * 0.5f + 0.5f;
        public IParty Party => _party;
        public ICharacterSheet GetSheet(CharacterId id) => _game.Sheets.TryGetValue(id, out var sheet) ? sheet : null;
        public short GetTicker(TickerId id) => _game.Tickers.TryGetValue(id, out var value) ? value : (short)0;
        public bool GetSwitch(SwitchId id) => _game.GetFlag(id);

        public MapChangeCollection TemporaryMapChanges => _game.TemporaryMapChanges;
        public MapChangeCollection PermanentMapChanges => _game.PermanentMapChanges;
        public ActiveItems ActiveItems => _game.Misc.ActiveItems;
        public IList<NpcState> Npcs => _game.Npcs;
        public bool IsChainDisabled(AssetId chainSource, ushort chain) => _game.DisabledChains.Contains((chainSource, chain));

        public MapId MapId => _game.MapId;

        public GameState()
        {
            On<NewGameEvent>(e => NewGame(e.MapId, e.X, e.Y));
            On<LoadGameEvent>(e => LoadGame(e.Id));
            On<SaveGameEvent>(e => SaveGame(e.Id, e.Name));
            On<FastClockEvent>(e => TickCount += e.Frames);
            On<SetTimeEvent>(e =>
            {
                _game.ElapsedTime = e.Time - SavedGame.Epoch;
            });
            On<LoadMapEvent>(e =>
            {
                if (_game != null)
                    _game.MapId = e.MapId;
            });
            On<SwitchEvent>(e =>
            {
                _game.SetFlag(e.SwitchId, e.Operation switch
                {
                    SwitchOperation.Reset => false,
                    SwitchOperation.Set => true,
                    SwitchOperation.Toggle => !_game.GetFlag(e.SwitchId),
                    _ => false
                });
            });
            On<TickerEvent>(e =>
            {
                _game.Tickers.TryGetValue(e.TickerId, out var curValue);
                _game.Tickers[e.TickerId] = (byte)e.Operation.Apply(curValue, e.Amount, 0, 255);
            });
            On<ChangeTimeEvent>(_ => { });
            On<ActivateItemEvent>(ActivateItem);
            On<DisableEventChainEvent>(e =>
            {
                _game.DisabledChains.Add((e.ChainSource, e.ChainNumber));
            });

            AttachChild(new InventoryManager(GetWriteableInventory));
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
                    inventory = _game.Sheets.TryGetValue(id.ToAssetId(), out var member) ? member.Inventory : null;
                    break;
                case InventoryType.Chest: _game.Inventories.TryGetValue(id.ToAssetId(), out inventory); break;
                case InventoryType.Merchant: _game.Inventories.TryGetValue(id.ToAssetId(), out inventory); break;
                default:
                    throw new InvalidOperationException($"Unexpected inventory type requested: \"{id.Type}\"");
            }

            return inventory;
        }

        public int TickCount { get; private set; }
        public bool Loaded => _game != null;

        void NewGame(MapId mapId, ushort x, ushort y)
        {
            Raise(new ReloadAssetsEvent()); // Make sure we don't end up with cached assets from the last game.
            var assets = Resolve<IAssetManager>();
            _game = new SavedGame
            {
                MapId = mapId,
                PartyX = x,
                PartyY = y,
                ActiveMembers = { [0] = Base.PartyMember.Tom }
            };

            foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.Party))
                _game.Sheets.Add(id, assets.LoadSheet(id));

            foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.Npc))
                _game.Sheets.Add(id, assets.LoadSheet(id));

            foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.Chest))
                _game.Inventories.Add(id, assets.LoadInventory(id));

            foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(AssetType.Merchant))
                _game.Inventories.Add(id, assets.LoadInventory(id));

            InitialiseGame();
        }

        string IdToPath(ushort id)
        {
            var generalConfig = Resolve<IGeneralConfig>();
            // TODO: This path currently exists in two places: here and Game\Gui\Menus\PickSaveSlot.cs
            return generalConfig.ResolvePath($"$(SAVE)/SAVE.{id:D3}");
        }

        void LoadGame(ushort id)
        {
            _game = Resolve<IAssetManager>().LoadSavedGame(IdToPath(id));
            if (_game == null)
                return;

            InitialiseGame();
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

        void InitialiseGame()
        {
            _party?.Remove();
            _party = AttachChild(new Party(_game.Sheets, _game.ActiveMembers, GetWriteableInventory));
            Raise(new LoadMapEvent(_game.MapId));
            Raise(new StartClockEvent());
            Raise(new SetContextEvent(ContextType.Leader, _party.Leader.Id));
            Raise(new PartyChangedEvent());
            Raise(new PartyJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new CameraJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new PlayerEnteredTileEvent(_game.PartyX, _game.PartyY));
        }

        void ActivateItem(ActivateItemEvent e)
        {
            if (e.Item == Base.Item.Clock) _game.Misc.ActiveItems |= ActiveItems.Clock;
            else if (e.Item == Base.Item.Compass) _game.Misc.ActiveItems |= ActiveItems.Compass;
            else if (e.Item == Base.Item.MonsterEye) _game.Misc.ActiveItems |= ActiveItems.MonsterEye;
        }
    }
}
