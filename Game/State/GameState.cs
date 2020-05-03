using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.State
{
    public class GameState : ServiceComponent<IGameState>, IGameState
    {
        static readonly DateTime Epoch = new DateTime(2200, 1, 1, 0, 0, 0);
        SavedGame _game;
        Party _party;

        public DateTime Time => Epoch + new TimeSpan(_game.Days, _game.Hours, _game.Minutes, 0);
        IParty IGameState.Party => _party;
        Func<ChestId, IInventory> IGameState.GetChest => x => GetInventory(InventoryType.Chest, (int)x);
        Func<MerchantId, IInventory> IGameState.GetMerchant => x => GetInventory(InventoryType.Merchant, (int)x);

        Func<NpcCharacterId, ICharacterSheet> IGameState.GetNpc => 
            x => _game != null && _game.Npcs.TryGetValue(x, out var sheet) ? sheet : null;
        Func<PartyCharacterId, ICharacterSheet> IGameState.GetPartyMember => 
            x => _game != null &&_game.PartyMembers.TryGetValue(x, out var member) ? member : null;
        public Func<int, short> GetTicker => 
            x => _game != null &&_game.Tickers.TryGetValue(x, out var value) ? value : (short)0;
        public Func<int, bool> GetSwitch =>
            x => _game != null &&_game.Switches.TryGetValue(x, out var value) && value;

        public IList<MapChange> TemporaryMapChanges => _game.TemporaryMapChanges;
        public IList<MapChange> PermanentMapChanges => _game.PermanentMapChanges;
        public MapDataId MapId => _game?.MapId ?? 0;


        public GameState()
        {
            On<NewGameEvent>(e => NewGame(e.MapId, e.X, e.Y));
            On<LoadGameEvent>(e => LoadGame(e.Filename));
            On<SaveGameEvent>(e => SaveGame(e.Filename, e.Name));
            On<FastClockEvent>(e => TickCount += e.Frames);
            On<LoadMapEvent>(e =>
            {
                if (_game != null)
                    _game.MapId = e.MapId;
            });
            On<SetTemporarySwitchEvent>(e => _game.Switches[e.SwitchId] = e.Operation switch
            {
                SetTemporarySwitchEvent.SwitchOperation.Reset => false,
                SetTemporarySwitchEvent.SwitchOperation.Set => true,
                SetTemporarySwitchEvent.SwitchOperation.Toggle => _game.Switches.ContainsKey(e.SwitchId) ? !_game.Switches[e.SwitchId] : true,
                _ => false
            });
            On<SetTickerEvent>(e =>
            {
                _game.Tickers.TryGetValue(e.TickerId, out var curValue);
                _game.Tickers[e.TickerId] = (byte)e.Operation.Apply(curValue, e.Amount, 0, 255);
            });
            On<ChangeTimeEvent>(e => { });

            AttachChild(new InventoryManager(GetInventory));
        }

        Inventory GetInventory(InventoryType type, int id)
        {
            if (_game == null)
                return null;

            Inventory inventory;
            switch(type)
            {
                case InventoryType.Player:
                    inventory = _game.PartyMembers.TryGetValue((PartyCharacterId)id, out var member) ? member.Inventory : null;
                    break;
                case InventoryType.Chest: _game.Chests.TryGetValue((ChestId)id, out inventory); break;
                case InventoryType.Merchant: _game.Merchants.TryGetValue((MerchantId)id, out inventory); break;
                default:
                    throw new InvalidOperationException($"Unexpected inventory type requested: \"{type}\"");
            };
            ApiUtil.Assert(inventory?.InventoryType == type && inventory.InventoryId == id);
            return inventory;
        }

        public int TickCount { get; private set; }
        public bool Loaded => _game != null;

        void NewGame(MapDataId mapId, ushort x, ushort y)
        {
            var assets = Resolve<IAssetManager>();
            _game = new SavedGame
            {
                MapId = mapId,
                PartyX = x,
                PartyY = y
            };
            _game.ActiveMembers[0] = PartyCharacterId.Tom;

            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                _game.PartyMembers.Add(charId, assets.LoadCharacter(charId));

            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                _game.Npcs.Add(charId, assets.LoadCharacter(charId));

            foreach(ChestId id in Enum.GetValues(typeof(ChestId)))
                _game.Chests.Add(id, assets.LoadChest(id));

            foreach(MerchantId id in Enum.GetValues(typeof(MerchantId)))
                _game.Merchants.Add(id, assets.LoadMerchant(id));

            InitialiseGame();
        }

        void LoadGame(string filename)
        {
            var loader = AssetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            using var stream = File.Open(filename, FileMode.Open);
            using var br = new BinaryReader(stream);
            var save = loader.Serdes(
                null,
                new AlbionReader(br, stream.Length), "SavedGame", null);
            _game = save;
            InitialiseGame();
        }

        void SaveGame(string filename, string name)
        {
            if (_game == null)
                return;

            var loader = AssetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            _game.Name = name;

            for (int i = 0; i < SavedGame.MaxPartySize; i++)
                _game.ActiveMembers[i] = _party.StatusBarOrder.Count > i
                    ? _party.StatusBarOrder[i].Id
                    : (PartyCharacterId?)null;

            using var stream = File.Open(filename, FileMode.Create);
            using var bw = new BinaryWriter(stream);
            loader.Serdes(_game, new AlbionWriter(bw), name, null);
        }

        void InitialiseGame()
        {
            _party?.Detach();
            _party = AttachChild(new Party(_game.PartyMembers, _game.ActiveMembers));
            Raise(new LoadMapEvent(_game.MapId));
            // TODO: Replay map modification events from save
            Raise(new StartClockEvent());
            Raise(new PartyChangedEvent());
            Raise(new PartyJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new CameraJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new PlayerEnteredTileEvent(_game.PartyX, _game.PartyY));
        }
    }
}
