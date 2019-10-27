using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class StateManager : Component, IStateManager
    {
        GameState _state;

        public IGameState State => _state;
        public int FrameCount { get; private set; }
        public PaletteId PaletteId { get; private set; }
        public Vector3 CameraTilePosition => CameraPosition / TileSize;
        public Vector3 CameraPosition { get; set; }
        public Vector2 CameraDirection { get; private set; }
        public float CameraMagnification { get; private set; }
        public Vector3 TileSize { get; private set; } = Vector3.One;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<StateManager, UpdateEvent>((x, e) => { x.FrameCount += e.Frames; }),
            H<StateManager, SetTileSizeEvent>((x, e) => { x.TileSize = e.TileSize; }),
            H<StateManager, LoadPaletteEvent>((x, e) => { x.PaletteId = e.PaletteId; }),
            H<StateManager, SetCameraDirectionEvent>((x, e) => x.CameraDirection = new Vector2(e.Yaw, e.Pitch)),
            H<StateManager, SetCameraMagnificationEvent>((x, e) => x.CameraMagnification = e.Magnification),
            H<StateManager, NewGameEvent>((x,e) => x.NewGame())
        );

        void NewGame()
        {
            var assets = Resolve<IAssetManager>();
            _state = new GameState();

            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                _state.PartyMembers.Add(charId, assets.LoadCharacter(AssetType.PartyMember, charId));

            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                _state.Npcs.Add(charId, assets.LoadCharacter(AssetType.Npc, charId));

            foreach(ChestId id in Enum.GetValues(typeof(ChestId)))
                _state.Chests.Add(id, assets.LoadChest(id));

            foreach(MerchantId id in Enum.GetValues(typeof(MerchantId)))
                _state.Merchants.Add(id, assets.LoadMerchant(id));

            var party = new Party { Leader = PartyCharacterId.Tom };
            party.Players.Add(new Player { Id = PartyCharacterId.Tom,      Position = Vector2.Zero });
            party.Players.Add(new Player { Id = PartyCharacterId.Rainer,   Position = Vector2.Zero });
            party.Players.Add(new Player { Id = PartyCharacterId.Drirr,    Position = Vector2.Zero });
            party.Players.Add(new Player { Id = PartyCharacterId.Sira,     Position = Vector2.Zero });
            party.Players.Add(new Player { Id = PartyCharacterId.Mellthas, Position = Vector2.Zero });
            party.Players.Add(new Player { Id = PartyCharacterId.Khunag,   Position = Vector2.Zero });

            _state.Party = party;
            Raise(new ReloadAssetsEvent()); // No need to keep character info cached once we've loaded it. New game is also a good point to clear out state.
            Raise(new PartyChangedEvent());
            Raise(new LoadMapEvent(MapDataId.Toronto2DGesamtkarteSpielbeginn));
        }

        public StateManager() : base(Handlers) { }
    }
}