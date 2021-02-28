using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game;

namespace UAlbion
{
    /// <summary>
    /// Used for pulling palettes from one mod context so another can save PNGs using them.
    /// Needed when unpacking the original XLDs.
    /// </summary>
    class PaletteHackAssetManager : ServiceComponent<IAssetManager>, IAssetManager
    {
        readonly Func<PaletteId, AlbionPalette> _loadFunc;
        public PaletteHackAssetManager(Func<PaletteId, AlbionPalette> loadFunc) => _loadFunc = loadFunc ?? throw new ArgumentNullException(nameof(loadFunc));
        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = _loadFunc(id);
            if (palette == null)
                return null;

            var commonId = AssetId.From(Base.Palette.CommonPalette);
            if (palette.Id != commonId.ToUInt32())
            {
                var commonPalette = _loadFunc(commonId);
                palette.SetCommonPalette(commonPalette);
            }

            return palette;
        }

        public ITexture LoadTexture(ITextureId id) => throw new NotImplementedException();
        public AssetInfo GetAssetInfo(AssetId id) => throw new NotImplementedException();
        public ITexture LoadTexture(SpriteId id) => throw new NotImplementedException();
        public ITexture LoadFont(FontColor color, bool isBold) => throw new NotImplementedException();
        public TilesetData LoadTileData(TilesetId id) => throw new NotImplementedException();
        public LabyrinthData LoadLabyrinthData(LabyrinthId id) => throw new NotImplementedException();
        public bool IsStringDefined(TextId id, GameLanguage? language) => throw new NotImplementedException();
        public bool IsStringDefined(StringId id, GameLanguage? language) => throw new NotImplementedException();
        public string LoadString(TextId id) => throw new NotImplementedException();
        public string LoadString(StringId id) => throw new NotImplementedException();
        public ISample LoadSample(SampleId id) => throw new NotImplementedException();
        public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => throw new NotImplementedException();
        public FlicFile LoadVideo(VideoId id) => throw new NotImplementedException();
        public IMapData LoadMap(MapId id) => throw new NotImplementedException();
        public ItemData LoadItem(ItemId id) => throw new NotImplementedException();
        public CharacterSheet LoadSheet(CharacterId id) => throw new NotImplementedException();
        public Inventory LoadInventory(AssetId id) => throw new NotImplementedException();
        public IList<Block> LoadBlockList(BlockListId id) => throw new NotImplementedException();
        public EventSet LoadEventSet(EventSetId id) => throw new NotImplementedException();
        public byte[] LoadSong(SongId id) => throw new NotImplementedException();
        public IList<IEvent> LoadScript(ScriptId id) => throw new NotImplementedException();
        public SpellData LoadSpell(SpellId id) => throw new NotImplementedException();
        public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();
        public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => throw new NotImplementedException();
        public Automap LoadAutomap(AutomapId id) => throw new NotImplementedException();
        public byte[] LoadSoundBanks() => throw new NotImplementedException();
    }
}