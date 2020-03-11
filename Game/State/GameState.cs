using System;
using System.IO;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class GameState : ServiceComponent<IGameState>, IGameState
    {
        static readonly DateTime Epoch = new DateTime(2200, 1, 1, 0, 0, 0);
        SavedGame _game;
        Party _party;

        public DateTime Time => Epoch + new TimeSpan(_game.Days, _game.Hours, _game.Minutes, 0);
        IParty IGameState.Party => _party;
        Func<NpcCharacterId, ICharacterSheet> IGameState.GetNpc => x => _game.Npcs[x];
        Func<ChestId, IChest> IGameState.GetChest => x => _game.Chests[x];
        Func<MerchantId, IChest> IGameState.GetMerchant => x => _game.Merchants[x];
        public Func<int, int> GetTicker => x => _game.Tickers.TryGetValue(x, out var value) ? value : 0;
        public Func<int, int> GetSwitch  => x => _game.Switches.TryGetValue(x, out var value) ? value : 0;
        public MapDataId MapId => _game.MapId;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<GameState, NewGameEvent>((x, e) => x.NewGame()),
            H<GameState, LoadGameEvent>((x, e) => x.LoadGame(e.Filename)),
            H<GameState, SaveGameEvent>((x, e) => x.SaveGame(e.Filename, e.Name)),
            H<GameState, UpdateEvent>((x, e) => x.TickCount += e.Frames),
            H<GameState, LoadMapEvent>((x, e) => x._game.MapId = e.MapId),
            H<GameState, SetTemporarySwitchEvent>((x, e) => { }),
            H<GameState, SetTickerEvent>((x, e) => { }),
            H<GameState, ChangeTimeEvent>((x, e) => { })
        );

        public GameState() : base(Handlers)
        {
            AttachChild(new InventoryScreenState());
        }

        public int TickCount { get; private set; }
        public bool Loaded => _game != null;

        void NewGame()
        {
            var assets = Resolve<IAssetManager>();
            _game = new SavedGame
            {
                MapId = MapDataId.Toronto2DGesamtkarteSpielbeginn,
                PartyX = 30,
                PartyY = 75
            };

            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                _game.PartyMembers.Add(charId, assets.LoadCharacter(AssetType.PartyMember, charId));

            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                _game.Npcs.Add(charId, assets.LoadCharacter(AssetType.Npc, charId));

            foreach(ChestId id in Enum.GetValues(typeof(ChestId)))
                _game.Chests.Add(id, assets.LoadChest(id));

            foreach(MerchantId id in Enum.GetValues(typeof(MerchantId)))
                _game.Merchants.Add(id, assets.LoadMerchant(id));

            _party?.Detach();
            _party = AttachChild(new Party(_game.PartyMembers));
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
            using var stream = File.Open(filename, FileMode.Create);
            using var bw = new BinaryWriter(stream);
            loader.Serdes(_game, new AlbionWriter(bw), name, null);
        }

        void InitialiseGame()
        {
            _party?.Detach();
            _party = AttachChild(new Party(_game.PartyMembers));
            Raise(new AddPartyMemberEvent(PartyCharacterId.Tom));
            Raise(new LoadMapEvent(_game.MapId));
            // TODO: Replay map modification events from save
            Raise(new StartClockEvent());
            Raise(new PartyJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new CameraJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new PlayerEnteredTileEvent(_game.PartyX, _game.PartyY));
        }
    }
}
