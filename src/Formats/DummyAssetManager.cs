using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats;

public class DummyAssetManager : IAssetManager
{
    static readonly AlbionPalette Greyscale = new(0, "Greyscale", Enumerable.Range(0, 256).Select(x => ApiUtil.PackColor((byte)x, (byte)x, (byte)x, 255)).ToArray());
    public AlbionPalette LoadPalette(PaletteId id) => Greyscale;
    public AssetInfo GetAssetInfo(AssetId id, string? language = null) => throw new NotImplementedException();
    public Automap LoadAutomap(AutomapId id) => throw new NotImplementedException();
    public CharacterSheet LoadSheet(CharacterId id) => throw new NotImplementedException();
    public EventSet LoadEventSet(EventSetId id) => throw new NotImplementedException();
    public FlicFile LoadVideo(VideoId id) => throw new NotImplementedException();
    public IList<Block> LoadBlockList(BlockListId id) => throw new NotImplementedException();
    public IList<IEvent> LoadScript(ScriptId id) => throw new NotImplementedException();
    public IMapData LoadMap(MapId id) => throw new NotImplementedException();
    public ISample LoadSample(SampleId id) => throw new NotImplementedException();
    public ITexture LoadFont(FontColor color, bool isBold) => throw new NotImplementedException();
    public ITexture LoadTexture(IAssetId id) => throw new NotImplementedException();
    public ITexture LoadTexture(SpriteId id) => throw new NotImplementedException();
    public ITileGraphics LoadTileGraphics(TilesetGraphicsId id) => throw new NotImplementedException();
    public Inventory LoadInventory(AssetId id) => throw new NotImplementedException();
    public ItemData LoadItem(ItemId id) => throw new NotImplementedException();
    public LabyrinthData LoadLabyrinthData(LabyrinthId id) => throw new NotImplementedException();
    public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => throw new NotImplementedException();
    public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();
    public SpellData LoadSpell(SpellId id) => throw new NotImplementedException();
    public TilesetData LoadTileData(TilesetId id) => throw new NotImplementedException();
    public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => throw new NotImplementedException();
    public bool IsStringDefined(StringId id, string language) => throw new NotImplementedException();
    public bool IsStringDefined(TextId id, string language) => throw new NotImplementedException();
    public byte[] LoadSong(SongId id) => throw new NotImplementedException();
    public byte[] LoadSoundBanks() => throw new NotImplementedException();
    public string LoadString(StringId id) => throw new NotImplementedException();
    public string LoadString(StringId id, string language) => throw new NotImplementedException();
    public string LoadString(TextId id) => throw new NotImplementedException();
    public string LoadString(TextId id, string language) => throw new NotImplementedException();
}